using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mara.Plugins.Mediator.Messaging;
using Mara.Plugins.Mediator;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;
using Remora.Results;
using Mara.Common;

[assembly:RemoraPlugin(typeof(MediatorPlugin))]

namespace Mara.Plugins.Mediator
{
    public sealed class MediatorPlugin : PluginDescriptor, ISkippedPlugin
    {
        /// <inheritdoc />
        public override string Name => nameof(Mediator);
        /// <inheritdoc />
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
        /// <inheritdoc />
        public override string Description
            => "Transforms Remora.Discord events into MediatR Notifications and Requests.";

        /// <inheritdoc />
        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddMessagingScoped<MediatorPlugin, ScopedMediator>();
        }
    }
}
