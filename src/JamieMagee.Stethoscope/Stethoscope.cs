namespace JamieMagee.Stethoscope;

using System.Runtime.InteropServices.ComTypes;
using System.Text.Json;
using Docker.DotNet;
using Docker.DotNet.Models;
using JamieMagee.Stethoscope.Models;
using Microsoft.Extensions.Logging;
using SharpCompress.Common;
using SharpCompress.Common.Tar;
using SharpCompress.Readers;
using SharpCompress.Readers.Tar;

public class Stethoscope : IStethoscope
{
    private readonly ILogger<Stethoscope> logger;
    private readonly IDockerClient client;

    private static readonly string StethoscopeTempPath = Path.Combine(Path.GetTempPath(), "stethoscope");
    private static readonly string LayersTempPath = Path.Combine(StethoscopeTempPath, "layers");
    private static readonly string ImagesTempPath = Path.Combine(StethoscopeTempPath, "images");

    // See https://github.com/opencontainers/image-spec/blob/main/layer.md#whiteouts
    private const string WhiteoutMarkerPrefix = ".wh.";
    private const string OpaqueWhiteoutMarker = ".wh..wh..opq";

    public Stethoscope(ILogger<Stethoscope> logger)
    {
        this.logger = logger;
        this.client = new DockerClientConfiguration().CreateClient();
    }

    public async Task SaveImageLayersToDiskAsync(CancellationToken cancellationToken = default)
    {
        const string image = "mcr.microsoft.com/dotnet/sdk:7.0-alpine";
        this.logger.LogInformation("Hello world!");

        if (!await this.ImageExistsLocallyAsync(image, cancellationToken))
        {
            await this.TryPullImageAsync(image, cancellationToken);
        }

        var tempDirectory = Path.Join(Path.GetTempPath(), "stethoscope");
        if (!Directory.Exists(tempDirectory))
        {
            _ = Directory.CreateDirectory(tempDirectory);
        }

        var imageHistory = await this.client.Images.GetImageHistoryAsync(image, cancellationToken);

        await using var imageStream = await this.client.Images.SaveImageAsync(image, cancellationToken);
        await using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        using var reader = ReaderFactory.Open(memoryStream);

        IEnumerable<Manifest> manifests = null;
        IList<string> layers = new List<string>();

        var imageDirectory = image;
        foreach (var invalidChar in Path.GetInvalidPathChars())
        {
            imageDirectory = imageDirectory.Replace(invalidChar, '-');
        }

        var destPath = Path.Join(ImagesTempPath, imageDirectory);
        if (!Directory.Exists(destPath))
        {
            Directory.CreateDirectory(destPath);
        }
        this.logger.LogInformation($"Will unpack to {destPath}");

        while (reader.MoveToNextEntry())
        {
            this.logger.LogInformation(reader.Entry.Key);
            switch (reader.Entry.Key)
            {
                case "manifest.json":
                {
                    await using var entryStream = reader.OpenEntryStream();
                    manifests = await JsonSerializer.DeserializeAsync<IEnumerable<Manifest>>(entryStream, cancellationToken: cancellationToken);
                    break;
                }

                case var _ when reader.Entry.Key.EndsWith("layer.tar", StringComparison.OrdinalIgnoreCase):
                {
                    var layerPath = Path.Join(LayersTempPath, reader.Entry.Key.Split('/')[0]);

                    if (Directory.Exists(layerPath))
                    {
                        this.logger.LogInformation($"Using cached layer on disk at {layerPath}");
                    }
                    else
                    {
                        this.logger.LogInformation($"Extracting layer to {layerPath}");
                        await using var layerStream = reader.OpenEntryStream();
                        this.ExtractLayerAsync(layerStream, layerPath);
                    }

                    this.logger.LogInformation($"Applying layer {reader.Entry.Key.Split('/')[0]} to {destPath}");
                    this.ApplyLayer(layerPath, destPath);
                    break;
                }
            }
        }
    }

    private void ExtractLayerAsync(Stream stream, string layerDir)
    {
        var reader = TarReader.Open(stream);
        while (reader.MoveToNextEntry())
        {
            if (reader.Entry.IsDirectory)
            {
                var directoryPath = Path.Combine(layerDir, reader.Entry.Key);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                continue;
            }

            var entryName = reader.Entry.Key;
            var entryDirName = Path.GetDirectoryName(entryName) ?? string.Empty;
            var entryFileName = Path.GetFileName(entryName);

            foreach (var invalidChar in Path.GetInvalidPathChars())
            {
                entryDirName = entryDirName.Replace(invalidChar, '-');
            }

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                entryFileName = entryFileName.Replace(invalidChar, '-');
            }

            entryName = Path.Combine(entryDirName, entryFileName);
            ExtractTarEntry(layerDir, reader, entryName);
        }
    }

    private static async Task ExtractTarEntry(string workingDir, TarReader reader, string entryName)
    {
        var filePath = Path.Combine(workingDir, entryName);
        var directoryPath = Path.GetDirectoryName(filePath);
        if (directoryPath is not null && !Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        if (!string.IsNullOrEmpty(reader.Entry.LinkTarget))
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var linkTarget = reader.Entry.LinkTarget;

            if (reader.Entry.LinkTarget.StartsWith('/'))
            {
                var slashes = reader.Entry.Key.Count(l => l == '/') - 1;
                linkTarget = string.Empty;
                for (var i = 0; i <= slashes; i++)
                {
                    linkTarget = Path.Join(linkTarget, "..");
                }

                linkTarget += reader.Entry.LinkTarget;
            }

            File.CreateSymbolicLink(filePath, linkTarget);
        }
        else
        {
            await using var entryStream = reader.OpenEntryStream();
            await using var outputStream = File.Create(filePath);
            await entryStream.CopyToAsync(outputStream, CancellationToken.None);
        }
    }

    private void ApplyLayer(string layerDir, string workingDir)
    {
        this.logger.LogInformation("Applying layer");

        var layerFiles = new DirectoryInfo(layerDir).GetFiles("*", SearchOption.AllDirectories);

        foreach (var layerFile in layerFiles)
        {
            var layerFileRelativePath = Path.GetRelativePath(layerDir, layerFile.FullName);
            var layerFileDirName = Path.GetDirectoryName(layerFileRelativePath);

            if (string.Equals(layerFile.Name, OpaqueWhiteoutMarker, StringComparison.Ordinal))
            {
                if (string.IsNullOrEmpty(layerFileDirName))
                {
                    throw new Exception("The opaque whiteout file marker should not exist in the root directory.");
                }

                var fullDirPath = Path.Combine(workingDir, layerFileDirName);

                if (Directory.Exists(fullDirPath))
                {
                    Directory.Delete(fullDirPath, recursive: true);
                }
            }
            else if (layerFile.Name.StartsWith(WhiteoutMarkerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var actualFileName = layerFile.Name[WhiteoutMarkerPrefix.Length..];
                var fullFilePath = Path.Combine(
                    workingDir,
                    Path.GetDirectoryName(layerFileDirName) ?? string.Empty,
                    actualFileName);

                if (File.Exists(fullFilePath))
                {
                    File.Delete(fullFilePath);
                }
            }
            else
            {
                var dest = Path.Combine(workingDir, layerFileRelativePath);
                var destDir = Path.GetDirectoryName(dest)!;
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                if (layerFile.LinkTarget is not null)
                {
                    if (File.Exists(dest))
                    {
                        File.Delete(dest);
                    }

                    File.CreateSymbolicLink(dest, layerFile.LinkTarget);
                }
                else
                {
                    File.Copy(layerFile.FullName, dest, overwrite: true);
                }
            }
        }
    }

    private async Task<bool> ImageExistsLocallyAsync(string image, CancellationToken cancellationToken = default)
    {
        try
        {
            await this.client.Images.InspectImageAsync(image, cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    private async Task<bool> TryPullImageAsync(string image, CancellationToken cancellationToken = default)
    {
        var parameters = new ImagesCreateParameters
        {
            FromImage = image,
        };
        try
        {
            var progress = new Progress<JSONMessage>(message =>
            {
                this.logger.LogInformation(JsonSerializer.Serialize(message));
            });
            await this.client.Images.CreateImageAsync(parameters, null, progress, cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<ImageInspectResponse> InspectImageAsync(string image, CancellationToken cancellationToken = default)
    {
        try
        {
            return await this.client.Images.InspectImageAsync(image, cancellationToken);
        }
        catch (Exception e)
        {
            return null;
        }
    }
}
