namespace JamieMagee.Stethoscope.Catalogers.Dpkg;

using Microsoft.Extensions.Logging;

public class DpkgCataloger : ICataloger
{
    private readonly ILogger<DpkgCataloger> logger;

    public DpkgCataloger(ILogger<DpkgCataloger> logger) => this.logger = logger;

    public string Globs => "**/var/lib/dpkg/status";

    public async Task<IEnumerable<IPackageMetadata>> RunAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        var reader = new StreamReader(stream);
        return await DpkgDatabaseParser.ParseAsync(reader);
    }
}
