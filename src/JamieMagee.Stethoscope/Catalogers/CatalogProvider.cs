namespace JamieMagee.Stethoscope.Catalogers;

using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;

public class CatalogProvider : ICatalogProvider
{
    private readonly IEnumerable<ICataloger> catalogers;
    private readonly ILogger<CatalogProvider> logger;

    public CatalogProvider(IEnumerable<ICataloger> catalogers, ILogger<CatalogProvider> logger)
    {
        this.catalogers = catalogers;
        this.logger = logger;
    }

    public async Task<IEnumerable<IPackageMetadata>> CatalogAsync(string location, CancellationToken cancellationToken = default)
    {
        var packages = new List<IPackageMetadata>();
        foreach (var cataloger in this.catalogers)
        {
            var result = new Matcher()
                .AddInclude(cataloger.Globs)
                .Execute(new DirectoryInfoWrapper(new DirectoryInfo(location)));

            if (result.HasMatches)
            {
                var fileStream = File.OpenRead(Path.Join(location, result.Files.First().Path));
                using var reader = new StreamReader(fileStream);
                packages.AddRange(await cataloger.RunAsync(reader, cancellationToken));
            }
        }

        return packages;
    }
}
