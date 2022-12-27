namespace JamieMagee.Stethoscope.Sources.DockerDaemon;

using System.Text.Json;
using Docker.DotNet;
using Docker.DotNet.Models;
using JamieMagee.Stethoscope.Models;
using Microsoft.Extensions.Logging;
using SharpCompress.Readers;
using SharpCompress.Readers.Tar;

public class DockerDaemonSource : Source, IDockerDaemonSource
{
    private readonly IDockerClient client;
    private readonly ILogger<DockerDaemonSource> logger;

    // See https://github.com/opencontainers/image-spec/blob/main/layer.md#whiteouts
    private const string WhiteoutMarkerPrefix = ".wh.";
    private const string OpaqueWhiteoutMarker = ".wh..wh..opq";

    public DockerDaemonSource(ILogger<DockerDaemonSource> logger)
    {
        this.logger = logger;
        this.client = new DockerClientConfiguration().CreateClient();
    }

    public override async Task<string> SaveImageAsync(string image, CancellationToken cancellationToken = default)
    {
        if (!await this.TryPullImageAsync(image, cancellationToken))
        {
            this.logger.LogCritical($"Can't pull image ${image}");
            throw new ArgumentException();
        }

        this.EnsureTempDirs();

        await using var imageStream = await this.client.Images.SaveImageAsync(image, cancellationToken);
        await using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        using var reader = ReaderFactory.Open(memoryStream);

        IEnumerable<Manifest> manifests = null;
        var destPath = this.EnsureImageDir(image);

        this.logger.LogInformation($"Will unpack to {destPath}");

        while (reader.MoveToNextEntry())
        {
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
                    var layerPath = Path.Join(Constants.LayersTempPath, reader.Entry.Key.Split('/')[0]);

                    if (Directory.Exists(layerPath))
                    {
                        this.logger.LogInformation($"Using cached layer on disk at {layerPath}");
                    }
                    else
                    {
                        this.logger.LogInformation($"Extracting layer to {layerPath}");
                        await using var layerStream = reader.OpenEntryStream();
                        await this.ExtractLayerAsync(layerStream, layerPath);
                    }

                    this.logger.LogInformation($"Applying layer {reader.Entry.Key.Split('/')[0]} to {destPath}");
                    this.ApplyLayer(layerPath, destPath);
                    break;
                }
            }
        }

        return destPath;
    }

    private async Task ExtractLayerAsync(Stream stream, string layerDir)
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
            await ExtractTarEntryAsync(layerDir, reader, entryName);
        }
    }

    private static async Task ExtractTarEntryAsync(string workingDir, TarReader reader, string entryName)
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
                this.logger.LogDebug(message.Status);
                this.logger.LogTrace(JsonSerializer.Serialize(message));
            });
            await this.client.Images.CreateImageAsync(parameters, null, progress, cancellationToken);

            return true;
        }
        catch (Exception e)
        {
            // ignored
        }

        return false;
    }
}
