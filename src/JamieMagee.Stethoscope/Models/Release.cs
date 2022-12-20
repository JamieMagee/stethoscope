namespace JamieMagee.Stethoscope.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Represents Linux Distribution release information.
/// See https://www.freedesktop.org/software/systemd/man/os-release.html.
/// </summary>
public sealed record Release
{
    [JsonPropertyName("prettyName")]
    public string PrettyName { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("idLike")]
    public IEnumerable<string> IdLike { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    [JsonPropertyName("versionId")]
    public string VersionId { get; set; }

    [JsonPropertyName("versionCodename")]
    public string VersionCodeName { get; set; }

    [JsonPropertyName("buildId")]
    public string BuildId { get; set; }

    [JsonPropertyName("imageId")]
    public string ImageId { get; set; }

    [JsonPropertyName("imageVersion")]
    public string ImageVersion { get; set; }

    [JsonPropertyName("variant")]
    public string Variant { get; set; }

    [JsonPropertyName("variantId")]
    public string VariantId { get; set; }

    [JsonPropertyName("homeUrl")]
    public string HomeUrl { get; set; }

    [JsonPropertyName("supportUrl")]
    public string SupportUrl { get; set; }

    [JsonPropertyName("bugReportUrl")]
    public string BugReportUrl { get; set; }

    [JsonPropertyName("PrivacyPolicyUrl")]
    public string PrivacyPolicyUrl { get; set; }

    [JsonPropertyName("cpeName")]
    public string CpeName { get; set; }

    public override string ToString()
    {
        if (this.PrettyName != string.Empty)
        {
            return this.PrettyName;
        }

        if (this.Name != string.Empty)
        {
            return this.Name;
        }

        if (this.Version != string.Empty)
        {
            return $"{this.Id} {this.Version}";
        }

        if (this.VersionId != string.Empty)
        {
            return $"{this.Id} {this.VersionId}";
        }

        return $"{this.Id} {this.BuildId}";
    }
}
