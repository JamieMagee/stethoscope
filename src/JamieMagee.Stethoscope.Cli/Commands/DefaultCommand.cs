namespace JamieMagee.Stethoscope.Cli.Commands;

using JamieMagee.Stethoscope.Cli.Settings;
using JamieMagee.Stethoscope.Parsers;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

public class DefaultCommand : AsyncCommand<DefaultCommandSettings>
{
    private readonly DockerDaemon dockerDaemon;
    private readonly ILogger<DefaultCommand> logger;

    public DefaultCommand(DockerDaemon dockerDaemon, ILogger<DefaultCommand> logger)
    {
        this.dockerDaemon = dockerDaemon;
        this.logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DefaultCommandSettings settings)
    {
        var location = await this.dockerDaemon.SaveImageLayersToDiskAsync(settings.Image);

        var apkDatabase =
            new Matcher()
                .AddInclude("**/lib/apk/db/installed")
                .Execute(new DirectoryInfoWrapper(new DirectoryInfo(location)));

        if (apkDatabase.HasMatches)
        {
            await using var stream = File.Open(Path.Join(location, apkDatabase.Files.First().Path), FileMode.Open);
            var reader = new StreamReader(stream);
            var results = await ApkDatabaseParser.ParseAsync(reader);
        }

        return 0;
    }
}
