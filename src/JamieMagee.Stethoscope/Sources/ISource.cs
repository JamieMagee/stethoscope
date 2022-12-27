namespace JamieMagee.Stethoscope.Sources;

public interface ISource
{
    Task<string> SaveImageAsync(string image, CancellationToken cancellationToken = default);
}
