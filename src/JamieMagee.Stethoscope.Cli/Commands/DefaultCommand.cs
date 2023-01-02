namespace JamieMagee.Stethoscope.Cli.Commands;

using JamieMagee.Stethoscope.Catalogers;
using JamieMagee.Stethoscope.Cli.Settings;
using JamieMagee.Stethoscope.OperatingSystems;
using JamieMagee.Stethoscope.Sources;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

public class DefaultCommand : AsyncCommand<DefaultCommandSettings>
{
    private readonly ICatalogProvider catalogProvider;
    private readonly IIdentifierProvider identifierProvider;
    private readonly ILogger<DefaultCommand> logger;
    private readonly ISourceFactory sourceFactory;

    public DefaultCommand(
        ISourceFactory sourceFactory,
        ICatalogProvider catalogProvider,
        IIdentifierProvider identifierProvider,
        ILogger<DefaultCommand> logger)
    {
        this.sourceFactory = sourceFactory;
        this.catalogProvider = catalogProvider;
        this.identifierProvider = identifierProvider;
        this.logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DefaultCommandSettings settings)
    {
        var (sourceProvider, image) = this.sourceFactory.GetSourceProvider(settings.Image);
        var location = await sourceProvider.SaveImageAsync(image);
        var packages = await this.catalogProvider.CatalogAsync(location);
        var release = await this.identifierProvider.IdentifyOperatingSystemAsync(location);

        ShowPackagesTable(packages);
        ShowRelease(release);

        return 0;
    }

    private static void ShowPackagesTable(IEnumerable<IPackageMetadata> packages)
    {
        var table = new Table();

        table.AddColumn("Package");
        table.AddColumn("Version");

        foreach (var package in packages)
        {
            table.AddRow(package.Package, package.Version);
        }

        AnsiConsole.Write(table);
    }

    private static void ShowRelease(IRelease release)
    {
        AnsiConsole.Write(new Markup($"[bold underline green]{release}[/]"));
    }
}
