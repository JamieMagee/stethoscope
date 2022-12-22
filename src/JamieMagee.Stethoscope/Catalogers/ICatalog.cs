namespace JamieMagee.Stethoscope.Catalogers;

public interface ICatalog
{
    Task CatalogAsync(string location, CancellationToken cancellationToken = default);
}
