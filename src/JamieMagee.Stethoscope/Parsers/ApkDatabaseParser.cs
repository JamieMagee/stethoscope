namespace JamieMagee.Stethoscope.Parsers;

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
                    case { } when line.StartsWith("V", StringComparison.Ordinal):
                        apk.Version = line[2..];
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
