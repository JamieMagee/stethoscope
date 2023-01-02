namespace JamieMagee.Stethoscope.OperatingSystems.Windows;

using System.Text.Json.Serialization;

public sealed record WindowsRelease : IRelease
{
    [JsonPropertyName("majorVersion")]
    public string MajorVersion { get; set; }

    [JsonPropertyName("minorVersion")]
    public string MinorVersion { get; set; }

    [JsonPropertyName("majorVersion")]
    public string BuildNumber { get; set; }

    [JsonPropertyName("buildRevision")]
    public string BuildRevision { get; set; }

    public override string ToString() =>
        $"{this.MajorVersion}.{this.MinorVersion}.{this.BuildNumber}.{this.BuildRevision}";
}
