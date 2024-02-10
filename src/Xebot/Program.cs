using System;
using System.Globalization;
using Discord.WebSocket;
using Xebot.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewRelic.LogEnrichers.Serilog;
using Serilog;
using Xebot.Converter;
using Discord;
using Xebot.BackgroundService;
using Discord.Interactions;
using Xebot.Storage;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var host = Host.CreateDefaultBuilder(args)
    .UseEnvironmentFromDotEnv()
    .ConfigureServices(services =>
    {
        services
            .AddSingleton<JsonConverter>()
            .AddSingleton<MemoryStorage>()
            
            .AddSingleton<InteractionService>()
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.All }))
            
            .AddHostedService<AutoEndService>()
            .AddHostedService<InteractionHandlingService>()
            .AddHostedService<DiscordStartupService>();
    })
    .UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .Enrich.WithNewRelicLogsInContext()
        .WriteTo.NewRelicLogs(
           licenseKey: Environment.GetEnvironmentVariable("NEW_RELIC_KEY"),
           endpointUrl: "https://log-api.eu.newrelic.com/log/v1",
           applicationName: "xebot")
    )
    .Build();

host.Run();