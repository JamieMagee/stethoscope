namespace JamieMagee.Stethoscope.Sources;

using DockerDaemon;
using JamieMagee.Stethoscope.Extensions;
using JamieMagee.Stethoscope.Sources.DockerRegistry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class SourceFactory : ISourceFactory
{
    private readonly ILogger<SourceFactory> logger;
    private readonly IServiceProvider serviceProvider;

    public SourceFactory(IServiceProvider serviceProvider, ILogger<SourceFactory> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    public (ISource Source, string Image) GetSourceProvider(string source)
    {
        var (sourceType, image) = ParseSourceType(source);
        return sourceType switch
        {
            SourceType.DockerDaemon => (this.serviceProvider.GetRequiredService<IDockerDaemonSource>(), image),
            SourceType.DockerRegistry => (this.serviceProvider.GetRequiredService<IDockerRegistrySource>(), image),
            SourceType.Unknown => throw new ArgumentException(source),
        };
    }

    private static (SourceType SourceType, string Image) ParseSourceType(string source)
    {
        var (sourceType, image, _) = source.Split("://");
        return sourceType switch
        {
            "docker-daemon" => (SourceType.DockerDaemon, image),
            "docker-registry" => (SourceType.DockerRegistry, image),
            _ => (SourceType.Unknown, string.Empty),
        };
    }
}
