namespace JamieMagee.Stethoscope.Catalogers;

public interface ICataloger
{
    public string Globs { get; }

    Task RunAsync(CancellationToken cancellationToken = default);
}
