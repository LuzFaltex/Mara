using Microsoft.Extensions.DependencyInjection;
using Mara.Plugins.Mediator.Messaging;
using Mara.Plugins.Mediator;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;

[assembly:RemoraPlugin(typeof(MediatorPlugin))]

namespace Mara.Plugins.Mediator
{
    public sealed class MediatorPlugin : PluginDescriptor
    {
        public override string Name => nameof(Mediator);

        public override string Description
            => "Transforms Remora.Discord events into MediatR Notifications and Requests";

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddMessagingScoped<MediatorPlugin, ScopedMediator>();
        }
    }
}
