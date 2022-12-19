namespace JamieMagee.Stethoscope;

public interface IStethoscope
{
    Task SaveImageLayersToDiskAsync(CancellationToken cancellationToken = default);
}
