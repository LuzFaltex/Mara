using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Mara.Common;
using Mara.Common.Models;
using Mara.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Gateway.Commands;
using Remora.Discord.API.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Gateway;
using Remora.Discord.Gateway.Extensions;
using Remora.Discord.Rest.Extensions;
using Remora.Plugins.Services;
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
                    var pluginService = new PluginService();
                    var plugins = pluginService.LoadAvailablePlugins().ToList();

                    Console.WriteLine($"Discovered {plugins.Count} plugins.");

                    services.Configure<MaraConfig>(context.Configuration);
                    services.AddSingleton(pluginService);

                    Debug.Assert(!string.IsNullOrEmpty(context.Configuration[nameof(MaraConfig.DiscordToken)]));

                    services.AddDiscordApi();
                    services.AddDiscordGateway(x => context.Configuration[nameof(MaraConfig.DiscordToken)]);
                    services.AddDiscordRest(x => context.Configuration[nameof(MaraConfig.DiscordToken)]);
                    services.AddCommands();
                    services.AddDiscordCommands(enableSlash: true);
                    services.AddMara();

                    services.Configure<DiscordGatewayClientOptions>(x
                        => x.Intents |=
                            GatewayIntents.DirectMessages |
                            GatewayIntents.GuildBans |
                            GatewayIntents.GuildIntegrations |
                            GatewayIntents.GuildInvites |
                            GatewayIntents.GuildMembers |
                            GatewayIntents.GuildMessageReactions);

                    foreach (var plugin in plugins)
                    {
                        if (plugin is ISkippedPlugin)
                            continue;

                        Console.Write($"Configuring {plugin.Name} version {plugin.Version.ToString(3)}...");
                        plugin.ConfigureServices(services);
                        Console.WriteLine("Done");
                    }
                })
                .ConfigureLogging((context, builder) =>
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
                })
                .UseConsoleLifetime();

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
