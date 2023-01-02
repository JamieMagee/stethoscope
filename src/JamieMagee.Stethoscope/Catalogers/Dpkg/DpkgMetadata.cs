namespace JamieMagee.Stethoscope.Catalogers.Dpkg;

using System.Text.Json.Serialization;

/// <summary>
/// http://manpages.ubuntu.com/manpages/xenial/man1/dpkg-query.1.html.
/// </summary>
public sealed record DpkgMetadata : IPackageMetadata
{
    [JsonPropertyName("package")]
    public string Package { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("sourceVersion")]
    public string SourceVersion { get; set; }

    [JsonPropertyName("architecture")]
    public string Architecture { get; set; }

    [JsonPropertyName("maintainer")]
    public string Maintainer { get; set; }

    [JsonPropertyName("installedSize")]
    public string InstalledSize { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("files")]
    public IEnumerable<string> Files { get; set; }
}
