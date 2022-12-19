namespace JamieMagee.Stethoscope.Cli.Commands;

using JamieMagee.Stethoscope.Cli.Settings;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

public class DefaultCommand : AsyncCommand<DefaultCommandSettings>
{
    private readonly IStethoscope stethoscope;
    private readonly ILogger<DefaultCommand> logger;

    public DefaultCommand(IStethoscope stethoscope, ILogger<DefaultCommand> logger)
    {
        this.stethoscope = stethoscope;
        this.logger = logger;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, DefaultCommandSettings settings)
    {
        this.logger.LogInformation("Hello from app");
        await this.stethoscope.SaveImageLayersToDiskAsync();

        return 0;
    }
}
