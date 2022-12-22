namespace JamieMagee.Stethoscope.Cli.Settings;

using Spectre.Console.Cli;

public class DefaultCommandSettings : CommandSettings
{
    [CommandArgument(0, "[image]")]
    public string Image { get; set; }
}
