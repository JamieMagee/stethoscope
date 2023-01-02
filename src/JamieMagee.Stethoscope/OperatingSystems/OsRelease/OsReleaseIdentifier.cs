namespace JamieMagee.Stethoscope.OperatingSystems.OsRelease;

using JamieMagee.Stethoscope.OperatingSystems;

public class OsReleaseIdentifier : IIdentifier
{
    public string Globs => "**/etc/os-release";

    public async Task<IRelease> RunAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var reader = new StreamReader(stream);
        return await OsReleaseParser.ParseOSReleaseAsync(reader, cancellationToken);
    }
}
