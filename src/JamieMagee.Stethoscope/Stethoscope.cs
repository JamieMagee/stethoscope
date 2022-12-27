namespace JamieMagee.Stethoscope;

using JamieMagee.Stethoscope.Catalogers;
using JamieMagee.Stethoscope.Catalogers.Apk;
using JamieMagee.Stethoscope.Catalogers.Dpkg;
using JamieMagee.Stethoscope.OperatingSystems;
using JamieMagee.Stethoscope.OperatingSystems.OsRelease;
using JamieMagee.Stethoscope.OperatingSystems.Windows;
using JamieMagee.Stethoscope.Sources;
using JamieMagee.Stethoscope.Sources.DockerDaemon;
using JamieMagee.Stethoscope.Sources.DockerRegistry;
using Microsoft.Extensions.DependencyInjection;

public static class Stethoscope
{
    public static IServiceCollection AddStethoscope(this IServiceCollection services)
    {
        services.AddSingleton<ISourceFactory, SourceFactory>();
        services.AddSingleton<IDockerDaemonSource, DockerDaemonSource>();
        services.AddSingleton<IDockerRegistrySource, DockerRegistrySource>();

        services.AddSingleton<ICatalogProvider, CatalogProvider>();
        services.AddSingleton<ICataloger, ApkCataloger>();
        services.AddSingleton<ICataloger, DpkgCataloger>();

        services.AddSingleton<IIdentifierProvider, IdentifierProvider>();
        services.AddSingleton<IIdentifier, OsReleaseIdentifier>();
        services.AddSingleton<IIdentifier, WindowsIdentifier>();

        return services;
    }
}
