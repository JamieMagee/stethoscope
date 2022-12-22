namespace JamieMagee.Stethoscope.Parsers;

using System.Globalization;
using JamieMagee.Stethoscope.Models;

public static class ApkDatabaseParser
{
    public static async Task<IEnumerable<ApkMetadata>> ParseAsync(StreamReader reader)
    {
        var apks = new List<ApkMetadata>();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            var apk = new ApkMetadata();
            while (!reader.EndOfStream && line != string.Empty)
            {
                switch (line)
                {
                    case { } when line.StartsWith("P", StringComparison.Ordinal):
                        apk.Package = line[2..];
                        break;
                    case { } when line.StartsWith("o", StringComparison.Ordinal):
                        apk.OriginPackage = line[2..];
                        break;
                    case { } when line.StartsWith("m", StringComparison.Ordinal):
                        apk.Maintainer = line[2..];
                        break;
                    case { } when line.StartsWith("V", StringComparison.Ordinal):
                        apk.Version = line[2..];
                        break;
                    case { } when line.StartsWith("L", StringComparison.Ordinal):
                        apk.License = line[2..];
                        break;
                    case { } when line.StartsWith("A", StringComparison.Ordinal):
                        apk.Architecture = line[2..];
                        break;
                    case { } when line.StartsWith("U", StringComparison.Ordinal):
                        apk.Url = line[2..];
                        break;
                    case { } when line.StartsWith("T", StringComparison.Ordinal):
                        apk.Description = line[2..];
                        break;
                    case { } when line.StartsWith("S", StringComparison.Ordinal):
                        apk.Size = long.Parse(line[2..], NumberFormatInfo.InvariantInfo);
                        break;
                    case { } when line.StartsWith("I", StringComparison.Ordinal):
                        apk.InstalledSize = long.Parse(line[2..], NumberFormatInfo.InvariantInfo);
                        break;
                    case { } when line.StartsWith("D", StringComparison.Ordinal):
                        apk.Dependencies.Add(line[2..]);
                        break;
                    case { } when line.StartsWith("p", StringComparison.Ordinal):
                        apk.Provides.Add(line[2..]);
                        break;
                    case { } when line.StartsWith("C", StringComparison.Ordinal):
                        apk.Checksum = line[2..];
                        break;
                    case { } when line.StartsWith("c", StringComparison.Ordinal):
                        apk.GitCommit = line[2..];
                        break;
                    default:
                        break;
                }

                line = await reader.ReadLineAsync();
            }

            apks.Add(apk);
        }

        return apks;
    }
}
