namespace JamieMagee.Stethoscope.Models;

using System.Text.Json.Serialization;
using JamieMagee.Stethoscope.Catalogers;

/// <summary>
/// ApkMetadata represents all captured data for a Alpine DB package entry.
/// See the following sources for more information:
/// - https://wiki.alpinelinux.org/wiki/Apk_spec
/// - https://git.alpinelinux.org/apk-tools/tree/src/package.c
/// - https://git.alpinelinux.org/apk-tools/tree/src/database.c
/// </summary>
/// TODO: Implement V2 properties https://wiki.alpinelinux.org/wiki/Apk_spec#Installed_Database_V2
public sealed record ApkMetadata : IPackageMetadata
{
    /// <summary>
    /// <code>P</code>
    /// Package name.
    /// </summary>
    [JsonPropertyName("package")]
    public string Package { get; set; }

    /// <summary>
    /// <code>o</code>
    /// Origin.
    /// </summary>
    [JsonPropertyName("originPackage")]
    public string OriginPackage { get; set; }

    /// <summary>
    /// <code>m</code>
    /// Maintainer.
    /// </summary>
    [JsonPropertyName("maintainer")]
    public string Maintainer { get; set; }

    /// <summary>
    /// <code>V</code>
    /// Package version.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// <code>L</code>
    /// License.
    /// </summary>
    [JsonPropertyName("license")]
    public string License { get; set; }

    [JsonPropertyName("architecture")]
    public string Architecture { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("size")]
    public long Size { get; set; }

    [JsonPropertyName("installedSize")]
    public long InstalledSize { get; set; }

    [JsonPropertyName("dependencies")]
    public IList<string> Dependencies { get; set; }

    [JsonPropertyName("provides")]
    public IList<string> Provides { get; set; }

    [JsonPropertyName("checksum")]
    public string Checksum { get; set; }

    [JsonPropertyName("gitCommit")]
    public string GitCommit { get; set; }
}
