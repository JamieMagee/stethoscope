namespace JamieMagee.Stethoscope.Catalogers.Apk;

using Microsoft.Extensions.Logging;

internal sealed class ApkCataloger : ICataloger
{
    private readonly ILogger<ApkCataloger> logger;

    public ApkCataloger(ILogger<ApkCataloger> logger)
    {
        this.logger = logger;
    }

    public string Globs => "**/lib/apk/db/installed";

    public async Task<IEnumerable<IPackageMetadata>> RunAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        var reader = new StreamReader(stream);
        return await ApkDatabaseParser.ParseAsync(reader);
    }
}
