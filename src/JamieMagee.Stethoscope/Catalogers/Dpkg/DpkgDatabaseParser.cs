namespace JamieMagee.Stethoscope.Catalogers.Dpkg;

using System.Text;
using System.Text.RegularExpressions;

public class DpkgDatabaseParser
{
    private static readonly Regex sourceRegex = new(@"(?<name>\S+)( \((?<version>.*)\))?");

    public static async Task<IEnumerable<DpkgMetadata>> ParseAsync(
        StreamReader reader,
        CancellationToken cancellationToken = default)
    {
        var packages = new List<DpkgMetadata>();
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            var dpkg = new DpkgMetadata();

            while (!reader.EndOfStream && line != string.Empty)
            {
                switch (line)
                {
                    case { } when line.StartsWith("Package:", StringComparison.Ordinal):
                        dpkg.Package = ExtractPropertyValue(line, "Package:");
                        break;
                    case { } when line.StartsWith("Source:", StringComparison.Ordinal):
                        dpkg.Source = ExtractPropertyValue(line, "Source:");
                        break;
                    case { } when line.StartsWith("Version:", StringComparison.Ordinal):
                        dpkg.Version = line[8..];
                        break;
                    case { } when line.StartsWith("Architecture:", StringComparison.Ordinal):
                        dpkg.Architecture = ExtractPropertyValue(line, "Architecture:");
                        break;
                    case { } when line.StartsWith("Maintainer:", StringComparison.Ordinal):
                        dpkg.Maintainer = ExtractPropertyValue(line, "Maintainer:");
                        break;
                    case { } when line.StartsWith("Installed-Size:", StringComparison.Ordinal):
                        dpkg.InstalledSize = ExtractPropertyValue(line, "Installed-Size:");
                        break;
                    case { } when line.StartsWith("Description:", StringComparison.Ordinal):
                        dpkg.Description = string.Join(await ExtractMultilinePropertyValueAsync(reader), ' ');
                        break;
                    case { } when line.StartsWith("Files:", StringComparison.Ordinal):
                        dpkg.Files = await ExtractMultilinePropertyValuesAsync(reader);
                        break;
                }

                line = await reader.ReadLineAsync();
            }

            if (!string.IsNullOrWhiteSpace(dpkg.Source))
            {
                var (sourceName, sourceVersion) = ExtractSourceVersion(dpkg.Source);
                if (!string.IsNullOrWhiteSpace(sourceVersion))
                {
                    dpkg.Source = sourceName;
                    dpkg.SourceVersion = sourceVersion;
                }
            }

            packages.Add(dpkg);
        }

        return packages;
    }

    private static string ExtractPropertyValue(string line, string propertyName) =>
        line.Substring(propertyName.Length).Trim();

    private static async Task<string> ExtractMultilinePropertyValueAsync(TextReader streamReader)
    {
        var value = new StringBuilder();
        while (streamReader.Peek() == ' ')
        {
            var line = await streamReader.ReadLineAsync();
            value.Append(line);
        }

        return value.ToString().TrimStart();
    }

    private static async Task<IList<string>> ExtractMultilinePropertyValuesAsync(TextReader streamReader)
    {
        var value = new List<string>();
        while (streamReader.Peek() == ' ')
        {
            var line = await streamReader.ReadLineAsync();
            value.Add(line.Trim());
        }

        return value;
    }

    private static (string SourceName, string SourceVersion) ExtractSourceVersion(string source)
    {
        var match = sourceRegex.Match(source);
        return (match.Groups["name"].Value, match.Groups["version"].Value);
    }
}
