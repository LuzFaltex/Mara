using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Mara.Common;
using Mara.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Services;

namespace Mara.Runtime
{
    public sealed class MaraBot : BackgroundService
    {
        private readonly DiscordGatewayClient _discordClient;
        private readonly IServiceProvider _services;
        private readonly MaraConfig _config;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<MaraBot> _logger;
        private readonly PluginService _pluginService;

        private IServiceScope _scope;

        public MaraBot
        (
            DiscordGatewayClient discordClient,
            IServiceProvider services,
            IOptions<MaraConfig> config,
            IHostApplicationLifetime applicationLifetime,
            IHostEnvironment environment,
            ILogger<MaraBot> logger,
            PluginService pluginService
        )
        {
            _discordClient = discordClient;
            _services = services;
            _applicationLifetime = applicationLifetime;
            _environment = environment;
            _logger = logger;
            _pluginService = pluginService;
            _config = config.Value;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            _logger.LogInformation("Starting bot service...");;

            IServiceScope? scope = null;
            try
            {
                // Create new scope for this session
                scope = _services.CreateScope();

                // Register the OnStopping method with the cancellation token
                stoppingToken.Register(OnStopping);

                // Load plugins
                var plugins = _pluginService.LoadAvailablePlugins();

                foreach (var plugin in plugins)
                {
                    if (plugin is ISkippedPlugin)
                        continue;

                    var serviceScope = _services.CreateScope();

                    if (plugin is IMigratablePlugin migratablePlugin)
                    {
                        if (await migratablePlugin.HasCreatedPersistentStoreAsync(serviceScope.ServiceProvider))
                        {
                            await migratablePlugin.MigratePluginAsync(serviceScope.ServiceProvider);
                        }
                    }

                    await plugin.InitializeAsync(serviceScope.ServiceProvider);
                }

                _scope = scope;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An error occurred while attempting to start the background service.");

                OnStopping();

                throw;
            }

            _logger.LogInformation("Logging into Discord and starting the client.");

            var runResult = await _discordClient.RunAsync(stoppingToken);

            if (!runResult.IsSuccess)
            {
                _logger.LogCritical("A critical error has occurred: {Error}", runResult.Error!.Message);
            }

            void OnStopping()
            {
                _logger.LogInformation("Stopping background service.");
                try
                {
                    _applicationLifetime.StopApplication();
                }
                finally
                {
                    scope?.Dispose();
                    _scope = null;
                }
            }
        }

        public override void Dispose()
        {
            try
            {
                base.Dispose();
            }
            finally
            {
                _scope?.Dispose();
            }
        }
    }
}
