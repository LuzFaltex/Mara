using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Mara.Common.Discord.Feedback;
using Mara.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Plugins.Services;
using Remora.Results;
using Serilog;
using Serilog.Events;

namespace Mara.Runtime
{
    public class Program
    {
        private static readonly string AppDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LuzFaltex", "Mara");

        private static readonly string LogDir = Path.Combine(AppDir, "..\\Logs");

        private const string DevEnvVar = "DOTNET_ENVIRONMENT";

        public static async Task<int> Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable(DevEnvVar) ?? "Production";

            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

            Console.WriteLine($"Mara version {fvi.FileVersion}");
            Console.WriteLine(fvi.LegalCopyright);
            Console.WriteLine("For internal use only.");

            var hostBuilder = new HostBuilder()
                .UseEnvironment(environment)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddEnvironmentVariables("Mara_");
                    builder.AddJsonFile("appsettings.json", true);
                    builder.AddJsonFile($"appsettings{context.HostingEnvironment.EnvironmentName}.json", true);
                    builder.AddJsonFile(Path.Combine(AppDir, "appsettings.json"), true);
                    builder.AddJsonFile(Path.Combine(AppDir, $"..\\appsettings{context.HostingEnvironment.EnvironmentName}.json"), true);

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddUserSecrets<Program>();
                    }
                })
                .UseDefaultServiceProvider(x => x.ValidateScopes = true)
                .ConfigureServices((context, services) =>
                {
                    var pluginServiceOptions = new PluginServiceOptions(new List<string>() { "./Plugins" });
                    var pluginService = new PluginService(Options.Create(pluginServiceOptions));

                    var plugins = pluginService.LoadPluginTree();
                    var configurePluginsResult = plugins.ConfigureServices(services);
                    if (!configurePluginsResult.IsSuccess)
                    {
                        Console.WriteLine($"Failed to load plugins! {configurePluginsResult.Error.Message}");
                        if (configurePluginsResult.Error is ExceptionError exe)
                        {
                            Console.WriteLine(exe.Exception.ToString());
                        }
                    }

                    services.Configure<MaraConfig>(context.Configuration);
                    services.AddSingleton(pluginService);
                    services.AddSingleton<IdentityInformationConfiguration>();

                    Debug.Assert(!string.IsNullOrEmpty(context.Configuration[nameof(MaraConfig.DiscordToken)]));

                    services.AddDiscordGateway(x => context.Configuration[nameof(MaraConfig.DiscordToken)]);
                    services.AddDiscordCommands(enableSlash: true);

                    services.AddMemoryCache();
                    services.AddHostedService<MaraBot>();

                    services.Configure<DiscordGatewayClientOptions>(x
                        => x.Intents |=
                            GatewayIntents.DirectMessages |
                            GatewayIntents.GuildBans |
                            GatewayIntents.GuildEmojisAndStickers |
                            GatewayIntents.GuildIntegrations |
                            GatewayIntents.GuildInvites |
                            GatewayIntents.GuildMembers |
                            GatewayIntents.GuildMessageReactions |
                            GatewayIntents.GuildMessages |
                            GatewayIntents.Guilds |
                            GatewayIntents.GuildWebhooks);
                })
                .ConfigureLogging((_, builder) =>
                {
                    Serilog.Core.Logger seriLogger = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .WriteTo.File(Path.Combine(LogDir, "Execution_.log"), rollingInterval: RollingInterval.Day)
                        .CreateLogger();

                    builder.AddSerilog(seriLogger);
                    Log.Logger = seriLogger;
                });

            hostBuilder = (Debugger.IsAttached && Environment.UserInteractive)
                ? hostBuilder.UseConsoleLifetime()
                : hostBuilder.UseWindowsService();

            using var host = hostBuilder.Build();

            if (!File.Exists(Path.Combine(AppDir, "appsettings.json")))
            {
                var config = MaraConfig.Default;
                await File.WriteAllTextAsync(Path.Combine(AppDir, "appsettings.json"),
                    JsonSerializer.Serialize(config, new JsonSerializerOptions {WriteIndented = true}));

                Console.WriteLine($"Default app config written to {Path.Combine(AppDir, "appsettings.json")}. Please provide your app configurations here or in environment variables, then restart the bot.");
                return 0;
            }

            try
            {
                await host.RunAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.ForContext<Program>()
                    .Fatal(ex, "Host terminated unexpectedly.");

                if (Debugger.IsAttached && Environment.UserInteractive)
                {
                    Console.WriteLine(Environment.NewLine + "Press any key to exit...");
                    Console.ReadKey(true);
                }

                return ex.HResult;
            }
        }
    }
}
