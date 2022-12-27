namespace JamieMagee.Stethoscope.Sources;

public interface ISourceFactory
{
    (ISource Source, string Image) GetSourceProvider(string source);
}
