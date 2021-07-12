using Microsoft.Extensions.DependencyInjection;
using Mara.Plugins.BetterEmbeds;
using Mara.Plugins.BetterEmbeds.API;
using Mara.Plugins.BetterEmbeds.MessageHandlers;
using Mara.Plugins.BetterEmbeds.Services;
using Remora.Discord.Gateway.Extensions;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;

[assembly:RemoraPlugin(typeof(BetterEmbedPlugin))]

namespace Mara.Plugins.BetterEmbeds
{
    public class BetterEmbedPlugin : PluginDescriptor
    {
        public override string Name => nameof(BetterEmbeds);
        public override string Description => "Provides improved embed functionality for links Discord handles poorly.";

        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IQuoteService, QuoteService>();
            serviceCollection.AddScoped<RedditRestAPI>();

            serviceCollection.AddResponder<QuoteEmbedHandler>();
            serviceCollection.AddResponder<RedditEmbedBuilder>();
        }
    }
}
