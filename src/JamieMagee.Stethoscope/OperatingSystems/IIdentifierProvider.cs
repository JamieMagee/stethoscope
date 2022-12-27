namespace JamieMagee.Stethoscope.OperatingSystems;

public interface IIdentifierProvider
{
    Task IdentifyOperatingSystemAsync(string location, CancellationToken cancellationToken = default);
}
