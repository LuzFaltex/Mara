using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mara.Common.Models;
using Mara.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Remora.Discord.API.Extensions;
using Remora.Discord.Commands.Extensions;
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

        private static readonly string LogDir = Path.Combine(AppDir, "logs");

        private const string DevEnvVar = "DOTNET_ENVIRONMENT";

        public static async Task<int> Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable(DevEnvVar) ?? "Production";

            var hostBuilder = new HostBuilder()
                .UseEnvironment(environment)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddEnvironmentVariables("Mara_");
                    builder.AddJsonFile("appsettings.json", true);
                    builder.AddJsonFile($"appsettings{context.HostingEnvironment.EnvironmentName}.json", true);

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddUserSecrets<Program>();
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    var pluginService = new PluginService();
                    var plugins = pluginService.LoadAvailablePlugins().ToList();

                    services.Configure<MaraConfig>(context.Configuration);
                    services.AddSingleton(pluginService);

                    services.AddDiscordApi();
                    services.AddDiscordGateway(x => context.Configuration[nameof(MaraConfig.DiscordToken)]);
                    services.AddDiscordRest(x => context.Configuration[nameof(MaraConfig.DiscordToken)]);
                    services.AddDiscordCommands(enableSlash: true);
                    services.AddMara();

                    foreach (var plugin in plugins)
                    {
                        plugin.ConfigureServices(services);
                    }
                })
                .ConfigureLogging((context, builder) =>
                {
                    Serilog.Core.Logger seriLogger = new LoggerConfiguration()
                        .MinimumLevel.Verbose()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .WriteTo.File(Path.Combine(LogDir, "Execution_.log"))
                        .CreateLogger();

                    builder.AddSerilog(seriLogger);
                    Log.Logger = seriLogger;
                })
                .UseConsoleLifetime();

            using var host = hostBuilder.Build();

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
