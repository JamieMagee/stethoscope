namespace JamieMagee.Stethoscope.Sources.DockerRegistry;

public class DockerRegistrySource : IDockerRegistrySource
{
    public Task<string> SaveImageAsync(string image, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
