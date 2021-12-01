using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Mara.Plugins.Core.Commands;
using Mara.Plugins.Core.Responders;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;
using Remora.Plugins.Abstractions;
using Remora.Results;

namespace Mara.Plugins.Core
{
    /// <summary>
    /// Represents core functionality.
    /// </summary>
    public class CorePlugin : PluginDescriptor
    {
        /// <inheritdoc />
        public override string Name => "Core";
        /// <inheritdoc />
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
        /// <inheritdoc />
        public override string Description => "Provides core functionality for the bot.";

        /// <inheritdoc />
        public override Result ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddResponder<ReadyResponder>();
            serviceCollection.AddResponder<SlashCommandRegistrationResponder>();
            serviceCollection.AddResponder<UnknownEventResponder>();
            serviceCollection.AddResponder<DeleteRequestResponder>();

            serviceCollection.AddCommandGroup<AboutCommand>();

            return Result.FromSuccess();
        }

        /// <inheritdoc />
        public override ValueTask<Result> InitializeAsync(IServiceProvider serviceProvider, CancellationToken ct = default)
        {
            return base.InitializeAsync(serviceProvider, ct);
        }
    }
}
