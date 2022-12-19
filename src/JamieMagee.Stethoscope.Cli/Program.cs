using JamieMagee.Stethoscope;
using JamieMagee.Stethoscope.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions.DependencyInjection;

Console.WriteLine("Hello");
var serviceCollection = new ServiceCollection()
    .AddLogging(configure =>
        configure.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.IncludeScopes = true;
        }))
    .AddSingleton<IStethoscope, Stethoscope>();
using var registrar = new DependencyInjectionRegistrar(serviceCollection);
var app = new CommandApp<DefaultCommand>(registrar);
return await app.RunAsync(args);
