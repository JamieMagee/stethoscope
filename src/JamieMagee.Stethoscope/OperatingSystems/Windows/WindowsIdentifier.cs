namespace JamieMagee.Stethoscope.OperatingSystems.Windows;

using JamieMagee.Stethoscope.OperatingSystems;
using Registry;

public class WindowsIdentifier : IIdentifier
{
    public string Globs => "**/Windows/System32/config/SOFTWARE";

    public async Task RunAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var hive = new RegistryHiveOnDemand(memoryStream.ToArray(), null);
        var currentVersion = hive.GetKey(@"Microsoft\Windows NT\CurrentVersion");
        var fullVersion =
            $"{currentVersion.GetValue("CurrentMajorVersionNumber")}.{currentVersion.GetValue("CurrentMinorVersionNumber")}.{currentVersion.GetValue("CurrentBuildNumber")}.{currentVersion.GetValue("UBR")}";
    }
}
