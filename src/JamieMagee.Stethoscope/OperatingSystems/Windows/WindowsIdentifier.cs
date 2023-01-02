namespace JamieMagee.Stethoscope.OperatingSystems.Windows;

using JamieMagee.Stethoscope.OperatingSystems;
using Registry;

public class WindowsIdentifier : IIdentifier
{
    public string Globs => "**/Windows/System32/config/SOFTWARE";

    public async Task<IRelease> RunAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var hive = new RegistryHiveOnDemand(memoryStream.ToArray(), null);
        var currentVersion = hive.GetKey(@"Microsoft\Windows NT\CurrentVersion");
        return new WindowsRelease
        {
            MajorVersion = currentVersion.GetValue("CurrentMajorVersionNumber").ToString(),
            MinorVersion = currentVersion.GetValue("CurrentMinorVersionNumber").ToString(),
            BuildNumber = currentVersion.GetValue("CurrentBuildNumber").ToString(),
            BuildRevision = currentVersion.GetValue("UBR").ToString(),
        };
    }
}
