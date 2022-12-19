namespace JamieMagee.Stethoscope;

using System.Text.Json;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
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

        var imageInspect = await this.InspectImageAsync(image, cancellationToken);
        var tempFile = Path.GetTempFileName();
        var imageStream = await this.client.Images.SaveImageAsync(image, cancellationToken);
        await using var tempStream = File.OpenWrite(tempFile);
        using var reader = ReaderFactory.Open(imageStream);
        while (reader.MoveToNextEntry())
        {
            this.logger.LogInformation(reader.Entry.Key);
            if (reader.Entry.Key.EndsWith(".tar"))
            {
                var layer = reader.OpenEntryStream();
                var layerReader = ReaderFactory.Open(layer);
                while (layerReader.MoveToNextEntry())
                {
                    this.logger.LogInformation(layerReader.Entry.Key);
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
            await this.client.Images.CreateImageAsync(parameters, null, null, cancellationToken);
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
