namespace JamieMagee.Stethoscope.Catalogers.Apk;

internal sealed class ApkCataloger : ICataloger
{
    public string Globs => "**/lib/apk/db/installed";

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
    }
}
