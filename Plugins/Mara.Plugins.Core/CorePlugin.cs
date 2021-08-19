using System;
using System.Reflection;
using System.Threading.Tasks;
using Mara.Plugins.Core;
using Mara.Plugins.Core.Commands;
using Mara.Plugins.Core.Responders;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Discord.Gateway.Extensions;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;
using Remora.Results;

[assembly:RemoraPlugin(typeof(CorePlugin))]

namespace Mara.Plugins.Core
{
    public class CorePlugin : PluginDescriptor
    {
        /// <inheritdoc />
        public override string Name => "Core";
        /// <inheritdoc />
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
        /// <inheritdoc />
        public override string Description => "Provides core functionality for the bot.";

        /// <inheritdoc />
        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddResponder<ReadyResponder>();
            serviceCollection.AddResponder<SlashCommandRegistrationResponder>();
            serviceCollection.AddResponder<UnknownEventResponder>();

            serviceCollection.AddCommandGroup<AboutCommand>();
        }

        /// <inheritdoc />
        public override ValueTask<Result> InitializeAsync(IServiceProvider serviceProvider)
        {
            return base.InitializeAsync(serviceProvider);
        }
    }
}
