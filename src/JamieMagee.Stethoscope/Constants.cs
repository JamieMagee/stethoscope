namespace JamieMagee.Stethoscope;

internal static class Constants
{
    internal static readonly string StethoscopeTempPath = Path.Combine(Path.GetTempPath(), "stethoscope");
    internal static readonly string LayersTempPath = Path.Combine(StethoscopeTempPath, "layers");
    internal static readonly string ImagesTempPath = Path.Combine(StethoscopeTempPath, "images");
}
