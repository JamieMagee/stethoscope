namespace JamieMagee.Stethoscope.OperatingSystems;

public interface IIdentifier
{
    public string Globs { get; }

    Task<IRelease> RunAsync(Stream stream, CancellationToken cancellationToken = default);
}
