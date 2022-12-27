namespace JamieMagee.Stethoscope.OperatingSystems.OsRelease;

using JamieMagee.Stethoscope.Extensions;
using JamieMagee.Stethoscope.Models;

public static class OsReleaseParser
{
    public static async Task<Release> ParseOSReleaseAsync(
        StreamReader reader,
        CancellationToken cancellationToken = default)
    {
        var release = new Release();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            var (key, value) = ParseLine(line);

            if (key == "ID_LIKE")
            {
                release.IdLike = value.Split(' ').Select(s => s.Trim());
            }

            switch (key)
            {
                case "PRETTY_NAME":
                    release.PrettyName = value;
                    break;
                case "NAME":
                    release.Name = value;
                    break;
                case "ID":
                    release.Id = value;
                    break;
                case "VERSION":
                    release.Version = value;
                    break;
                case "VERSION_ID":
                    release.VersionId = value;
                    break;
                case "VERSION_CODENAME":
                    release.VersionCodeName = value;
                    break;
                case "BUILD_ID":
                    release.BuildId = value;
                    break;
                case "IMAGE_ID":
                    release.ImageId = value;
                    break;
                case "IMAGE_VERSION":
                    release.ImageVersion = value;
                    break;
                case "VARIANT":
                    release.Variant = value;
                    break;
                case "VARIANT_ID":
                    release.VariantId = value;
                    break;
                case "HOME_URL":
                    release.HomeUrl = value;
                    break;
                case "SUPPORT_URL":
                    release.SupportUrl = value;
                    break;
                case "BUG_REPORT_URL":
                    release.BugReportUrl = value;
                    break;
                case "PRIVACY_POLICY_URL":
                    release.PrivacyPolicyUrl = value;
                    break;
                case "CPE_NAME":
                    release.CpeName = value;
                    break;
            }
        }

        return release;
    }

    private static (string Key, string Value) ParseLine(string line)
    {
        var (key, value, _) = line.Split('=');
        key = key.Trim(' ');
        value = value.Trim(' ');

        // Handle quotes
        if (value.Contains('"'))
        {
            var first = value[..1];
            var last = value[^1..];

            if (first == last && (first.Contains('"') || first.Contains('\'')))
            {
                value = value.TrimStart('"');
                value = value.TrimStart('\'');
                value = value.TrimEnd('"');
                value = value.TrimEnd('\'');
            }
        }

        // Expand anything else that could be escaped
        value = value.Replace(@"\\""", "\"");
        value = value.Replace(@"\$", "$");
        value = value.Replace(@"\\", "\\");
        value = value.Replace(@"\\`", "`");

        return (key, value);
    }
}
