namespace JamieMagee.Stethoscope.Catalogers;

public interface ICatalogProvider
{
    Task<IEnumerable<IPackageMetadata>> CatalogAsync(string location, CancellationToken cancellationToken = default);
}
