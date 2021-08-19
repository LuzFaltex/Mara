using System;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Mara.Plugins.BetterEmbeds;
using Mara.Plugins.BetterEmbeds.API;
using Mara.Plugins.BetterEmbeds.MessageHandlers;
using Mara.Plugins.BetterEmbeds.Models.OEmbed;
using Mara.Plugins.BetterEmbeds.Models.Reddit;
using Mara.Plugins.BetterEmbeds.Models.Reddit.Converters;
using Mara.Plugins.BetterEmbeds.Services;
using Remora.Discord.API.Extensions;
using Remora.Discord.API.Json;
using Remora.Discord.Gateway.Extensions;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;

[assembly:RemoraPlugin(typeof(BetterEmbedPlugin))]

namespace Mara.Plugins.BetterEmbeds
{
    /// <summary>
    /// Fixes Discord's automatic embeds of sites like Reddit.
    /// </summary>
    public class BetterEmbedPlugin : PluginDescriptor
    {
        /// <inheritdoc />
        public override string Name => nameof(BetterEmbeds);
        /// <inheritdoc />
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0);
        /// <inheritdoc />
        public override string Description => "Provides improved embed functionality for links Discord handles poorly.";

        /// <inheritdoc />
        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IQuoteService, QuoteService>();
            serviceCollection.AddScoped<RedditRestAPI>();

            serviceCollection.AddResponder<QuoteEmbedHandler>();
            serviceCollection.AddResponder<RedditEmbedBuilder>();

            serviceCollection.Configure<JsonSerializerOptions>(options =>
            {
                options.AddConverter<UtcTimestampDateTimeConverter>();
                options.AddConverter<UnixSecondsDateTimeOffsetConverter>();

                options.AddDataObjectConverter<IRedditPost, RedditPost>();
                options.AddDataObjectConverter<IRedditUser, RedditUser>();

                options.AddDataObjectConverter<IOEmbed, OEmbed>();
                options.AddDataObjectConverter<IPhoto, Photo>();
                options.AddDataObjectConverter<IVideo, Video>();
            });
        }
    }
}
