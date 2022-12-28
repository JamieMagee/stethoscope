namespace JamieMagee.Stethoscope.Catalogers;

public interface ICataloger
{
    public string Globs { get; }

    Task<IEnumerable<IPackageMetadata>> RunAsync(Stream stream, CancellationToken cancellationToken = default);
}
