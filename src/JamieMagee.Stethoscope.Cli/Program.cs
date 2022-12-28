using JamieMagee.Stethoscope;
using JamieMagee.Stethoscope.Cli.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Spectre.Console.Cli.Extensions.DependencyInjection;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true)
    .Build();

var serviceCollection = new ServiceCollection()
    .AddLogging(configure =>
        configure.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.IncludeScopes = true;
        }).AddConfiguration(config.GetSection("Logging")))
    .AddStethoscope();
using var registrar = new DependencyInjectionRegistrar(serviceCollection);
var app = new CommandApp<DefaultCommand>(registrar);
return await app.RunAsync(args);
