namespace JamieMagee.Stethoscope.Models;

using System.Text.Json.Serialization;

/// <summary>
/// ApkMetadata represents all captured data for a Alpine DB package entry.
/// See the following sources for more information:
/// - https://wiki.alpinelinux.org/wiki/Apk_spec
/// - https://git.alpinelinux.org/apk-tools/tree/src/package.c
/// - https://git.alpinelinux.org/apk-tools/tree/src/database.c
/// </summary>
public sealed record ApkMetadata
{
    [JsonPropertyName("package")]
    public string Package { get; set; }

    [JsonPropertyName("originPackage")]
    public string OriginPackage { get; set; }

    [JsonPropertyName("maintainer")]
    public string Maintainer { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

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
    public string[] Dependencies { get; set; }

    [JsonPropertyName("provides")]
    public string[] Provides { get; set; }

    [JsonPropertyName("checksum")]
    public string Checksum { get; set; }

    [JsonPropertyName("gitCommit")]
    public string GitCommit { get; set; }
}
