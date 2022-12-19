namespace JamieMagee.Stethoscope;

using System.Text.Json;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Models;
using SharpCompress.Common;
using SharpCompress.Readers;

public class Stethoscope : IStethoscope
{
    private readonly ILogger<Stethoscope> logger;
    private readonly IDockerClient client;

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
        const string image = "alpine:latest";
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

        var imageStream = await this.client.Images.SaveImageAsync(image, cancellationToken);
        using var reader = ReaderFactory.Open(imageStream);

        IEnumerable<Manifest> manifests = null;
        IList<string> layers = new List<string>();

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
                    await using var layerStream = reader.OpenEntryStream();
                    var layerPath = Path.Join(tempDirectory, reader.Entry.Key.Split('/')[0]);
                    if (!Directory.Exists(layerPath))
                    {
                        _ = Directory.CreateDirectory(layerPath);
                    }

                    using var layerReader = ReaderFactory.Open(layerStream);
                    while (layerReader.MoveToNextEntry())
                    {
                        if (!layerReader.Entry.IsDirectory)
                        {
                            layerReader.WriteEntryToDirectory(layerPath, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true,
                            });
                        }
                    }

                    layers.Add(layerPath);
                    break;
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
