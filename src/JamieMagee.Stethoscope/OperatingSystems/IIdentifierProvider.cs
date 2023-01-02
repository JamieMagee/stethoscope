namespace JamieMagee.Stethoscope.OperatingSystems;

public interface IIdentifierProvider
{
    Task<IRelease> IdentifyOperatingSystemAsync(string location, CancellationToken cancellationToken = default);
}
