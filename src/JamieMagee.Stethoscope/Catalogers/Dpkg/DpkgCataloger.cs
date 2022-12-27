namespace JamieMagee.Stethoscope.Catalogers.Dpkg;

using Microsoft.Extensions.Logging;

public class DpkgCataloger : ICataloger
{
    private readonly ILogger<DpkgCataloger> logger;

    public DpkgCataloger(ILogger<DpkgCataloger> logger) => this.logger = logger;

    public string Globs => "";

    public Task<IEnumerable<IPackageMetadata>> RunAsync(
        StreamReader reader,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
