using JamieMagee.Stethoscope;
using JamieMagee.Stethoscope.Cli.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection()
    .AddLogging(configure =>
        configure.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.IncludeScopes = true;
        }))
    .AddStethoscope();
using var registrar = new DependencyInjectionRegistrar(serviceCollection);
var app = new CommandApp<DefaultCommand>(registrar);
return await app.RunAsync(args);
