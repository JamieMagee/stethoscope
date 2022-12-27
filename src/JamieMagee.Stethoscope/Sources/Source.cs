namespace JamieMagee.Stethoscope.Sources;

public abstract class Source : ISource
{
    public abstract Task<string> SaveImageAsync(string image, CancellationToken cancellationToken = default);

    private protected void EnsureTempDirs()
    {
        if (!Directory.Exists(Constants.LayersTempPath))
        {
            _ = Directory.CreateDirectory(Constants.StethoscopeTempPath);
        }

        if (!Directory.Exists(Constants.ImagesTempPath))
        {
            _ = Directory.CreateDirectory(Constants.StethoscopeTempPath);
        }
    }

    private protected string EnsureImageDir(string image)
    {
        var imageDir = Path.Join(Constants.ImagesTempPath, image);
        if (!Directory.Exists(imageDir))
        {
            _ = Directory.CreateDirectory(imageDir);
        }

        return imageDir;
    }
}
