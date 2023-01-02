namespace JamieMagee.Stethoscope.Catalogers.Windows;

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Registry;

public class WindowsCataloger : ICataloger
{
    private readonly ILogger<WindowsCataloger> logger;

    private static readonly Regex updatePackageRegex = new(@"^Package_\d+_for_(KB\d+)~\w{16}~\w+~~((?:\d+\.){3}\d+)$");

    public WindowsCataloger(ILogger<WindowsCataloger> logger)
    {
        this.logger = logger;
    }

    public string Globs => "**/Windows/System32/config/SOFTWARE";

    public async Task<IEnumerable<IPackageMetadata>> RunAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var hive = new RegistryHiveOnDemand(memoryStream.ToArray(), null);
        var currentVersion = hive.GetKey(@"Microsoft\Windows\CurrentVersion\Component Based Servicing\Packages");

        var packages = new Dictionary<string, string>();
        foreach (var packageKey in currentVersion.SubKeys)
        {
            if (!updatePackageRegex.IsMatch(packageKey.KeyName))
            {
                continue;
            }

            var package = hive.GetKey(packageKey.KeyPath);
            var currentState = package.Values.Find(v => v.ValueName == "CurrentState")?.ValueData;

            // Installed
            if (currentState == "112")
            {
                var groups = updatePackageRegex.Match(package.KeyName).Groups;
                packages[groups[1].Value] = groups[2].Value;
            }
        }


        return Enumerable.Empty<IPackageMetadata>();
    }
}
