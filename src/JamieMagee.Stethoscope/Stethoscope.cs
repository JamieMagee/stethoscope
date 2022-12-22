namespace JamieMagee.Stethoscope;

using Microsoft.Extensions.DependencyInjection;

public static class Stethoscope
{
    public static IServiceCollection AddStethoscope(this IServiceCollection services)
    {
        services.AddSingleton<DockerDaemon>();

        return services;
    }
}
