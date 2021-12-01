using System;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Mara.Plugins.BetterEmbeds;
using Mara.Plugins.BetterEmbeds.MessageHandlers;
using Mara.Plugins.BetterEmbeds.Models.OEmbed;
using Mara.Plugins.BetterEmbeds.Models.Reddit;
using Mara.Plugins.BetterEmbeds.Models.Reddit.Converters;
using Mara.Plugins.BetterEmbeds.Services;
using Remora.Discord.Gateway.Extensions;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;
using Remora.Rest.Results;
using Remora.Rest.Extensions;
using Polly;
using System.Net.Http;
using Polly.Contrib.WaitAndRetry;
using Remora.Results;
using System.Threading.Tasks;
using Polly.Retry;

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
        public override Result ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IQuoteService, QuoteService>();
            // serviceCollection.AddRestHttpClient<RestHttpClient<RestResultError<HttpResultError>>>();

            var retryDelay = Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5);

            var clientBuilder = serviceCollection
                .AddRestHttpClient<RestResultError<HttpResultError>>("Reddit")
                .ConfigureHttpClient((services, client) =>
                {
                    var assemblyName = Assembly.GetExecutingAssembly().GetName();
                    var name = assemblyName.Name ?? "LuzFaltex.Mara";
                    var version = assemblyName.Version ?? new Version(1, 0, 0);

                    client.BaseAddress = new("https://www.reddit.com/");
                    client.DefaultRequestHeaders.UserAgent.Add
                    (
                        new System.Net.Http.Headers.ProductInfoHeaderValue(name, version.ToString())
                    );
                })
                .AddTransientHttpErrorPolicy
                (
                    b => b.WaitAndRetryAsync(retryDelay)
                )
                .AddPolicyHandler
                (
                    Policy
                    .HandleResult<HttpResponseMessage>(r => r.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    .WaitAndRetryAsync
                    (
                        1,
                        (iteration, response, context) =>
                        {
                            if (response.Result == default)
                            {
                                return TimeSpan.FromSeconds(1);
                            }

                            return (TimeSpan)(response.Result.Headers.RetryAfter is null or { Delta: null }
                                ? TimeSpan.FromSeconds(1)
                                : response.Result.Headers.RetryAfter.Delta);
                        },
                        (_, _, _, _) => Task.CompletedTask
                    )
                );

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

            return Result.FromSuccess();
        }        
    }
}
