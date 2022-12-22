namespace JamieMagee.Stethoscope.Catalogers;

using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;

public class Catalog : ICatalog
{
    private readonly IEnumerable<ICataloger> catalogers;
    private readonly ILogger<Catalog> logger;

    public Catalog(IEnumerable<ICataloger> catalogers, ILogger<Catalog> logger)
    {
        this.catalogers = catalogers;
        this.logger = logger;
    }

    public async Task CatalogAsync(string location, CancellationToken cancellationToken = default)
    {
        foreach (var cataloger in this.catalogers)
        {
            var result = new Matcher()
                .AddInclude(cataloger.Globs)
                .Execute(new DirectoryInfoWrapper(new DirectoryInfo(location)));

            if (result.HasMatches)
            {
                await cataloger.RunAsync(cancellationToken);
            }
        }
    }
}
