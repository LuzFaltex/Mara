using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Mara.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Remora.Discord.Gateway;
using Remora.Plugins.Services;
using Remora.Results;

namespace Mara.Runtime
{
    public sealed class MaraBot : BackgroundService
    {
        private readonly DiscordGatewayClient _discordClient;
        private readonly IServiceProvider _services;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ILogger<MaraBot> _logger;
        private readonly PluginService _pluginService;

        private IServiceScope? _scope = null;

        public MaraBot
        (
            DiscordGatewayClient discordClient,
            IServiceProvider services,
            IHostApplicationLifetime applicationLifetime,
            ILogger<MaraBot> logger,
            PluginService pluginService
        )
        {
            _discordClient = discordClient;
            _services = services;
            _applicationLifetime = applicationLifetime;
            _logger = logger;
            _pluginService = pluginService;
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-us");
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            var initResult = await InitializeAsync(stoppingToken);
            if (!initResult.IsSuccess)
            {
                _logger.LogError(initResult.Error);
                return;
            }            

            _logger.LogInformation("Logging into Discord and starting the client.");

            var runResult = await _discordClient.RunAsync(stoppingToken);

            if (!runResult.IsSuccess)
            {
                _logger.LogCritical("A critical error has occurred: {Error}", runResult.Error!.Message);
            }            
        }

        private async Task<Result> InitializeAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Initializing bot service...");
            
            try
            {
                // Create new scope for this session
                _scope = _services.CreateScope();

                // Register the OnStopping method with the cancellation token
                stoppingToken.Register(OnStopping);

                // Load plugins
                var pluginTree = _pluginService.LoadPluginTree();

                var initResult = await pluginTree.InitializeAsync(_scope.ServiceProvider, stoppingToken);
                if (!initResult.IsSuccess)
                {
                    return initResult;
                }

                var migrateResult = await pluginTree.MigrateAsync(_scope.ServiceProvider, stoppingToken);
                if (migrateResult.IsSuccess)
                {
                    return migrateResult;
                }

                return Result.FromSuccess();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An error occurred while attempting to start the background service.");

                OnStopping();

                return ex;
            }
        }

        private void OnStopping()
        {
            _logger.LogInformation("Stopping background service.");
            try
            {
                _applicationLifetime.StopApplication();
            }
            finally
            {
                _scope = null;
            }
        }

        /// <inheritdoc />
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
