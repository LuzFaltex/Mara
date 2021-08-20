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
                options.AddDataObjectConverter<IRedditPost, RedditPost>()
                    .WithPropertyName(x => x.Title, "title")
                    .WithPropertyName(x => x.Subreddit, "subreddit_name_prefixed")
                    .WithPropertyName(x => x.Author, "author")
                    .WithPropertyName(x => x.Url, "url")
                    .WithPropertyName(x => x.Permalink, "permalink")
                    .WithPropertyName(x => x.Text, "selftext")
                    .WithPropertyName(x => x.Score, "score")
                    .WithPropertyName(x => x.UpvoteRatio, "upvote_ratio")
                    .WithPropertyName(x => x.PostDate, "created_utc")
                    .WithPropertyName(x => x.PostFlair, "link_flair_text")
                    .WithPropertyName(x => x.Media, "media")
                    .WithPropertyName(x => x.IsVideo, "is_video")
                    .WithPropertyName(x => x.PostHint, "post_hint")
                    .WithPropertyName(x => x.WhitelistStatus, "whitelist_status")
                    .WithPropertyName(x => x.Thumbnail, "thumbnail")
                    .WithPropertyName(x => x.ThumbnailWidth, "thumbnail_width")
                    .WithPropertyName(x => x.ThumbnailHeight, "thumbnail_height")

                    .WithPropertyConverter(x => x.PostDate, new UtcTimestampDateTimeConverter());

                options.AddDataObjectConverter<IRedditUser, RedditUser>()
                    .WithPropertyName(x => x.DisplayNamePrefixed, "display_name_prefixed")
                    .WithPropertyName(x => x.IconImage, "icon_img");

                options.AddDataObjectConverter<IMedia, Media>()
                    .WithPropertyName(x => x.RedditVideo, "reddit_video")
                    .WithPropertyName(x => x.ExternalVideo, "oembed");

                options.AddDataObjectConverter<IOEmbed, OEmbed>();
                options.AddDataObjectConverter<IPhoto, Photo>();
                options.AddDataObjectConverter<IVideo, Video>();
            });
        }
    }
}
