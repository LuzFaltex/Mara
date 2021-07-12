using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Mara.Common.Discord;
using Mara.Common.Discord.Feedback.Errors;
using Mara.Plugins.BetterEmbeds.API;
using Mara.Plugins.BetterEmbeds.Models.Reddit;
using Mara.Plugins.BetterEmbeds.Results;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mara.Plugins.BetterEmbeds.MessageHandlers
{
    public sealed class RedditEmbedBuilder : BetterEmbedHandlerBase
    {
        private static readonly Regex UrlRegex = new Regex(
            @"(?<Prelink>\S+\s+\S*)?(?<OpenBrace><)?https?://(?:(?:www)\.)?reddit\.com/r/(?<Subreddit>[A-Za-z0-9_]+)/comments/(?<PostId>[a-z0-9]+)/(?<PostName>[a-z0-9_]+)?(?:/?(?:\?[a-z_=&]+)?)?(?<CloseBrace>>)?(?<PostLink>\S*\s+\S+)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private const string JsonUrl = "https://www.reddit.com/r/{0}/comments/{1}/.json";

        private readonly IDiscordRestChannelAPI _channelApi;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly RedditRestAPI _redditApi;

        public RedditEmbedBuilder(ILogger<RedditEmbedBuilder> logger, IDiscordRestChannelAPI channelApi, IDiscordRestGuildAPI guildApi, RedditRestAPI redditApi) : base(logger, UrlRegex, channelApi)
        {
            _channelApi = channelApi;
            _guildApi = guildApi;
            _redditApi = redditApi;
        }

        public override async ValueTask<Result<IEmbed>> BuildEmbedAsync(Match match, IMessage message, CancellationToken cancellationToken = default)
        {
            var subreddit = match.Groups["Subreddit"].Value;
            var postId = match.Groups["PostId"].Value;

            var redditResult = await _redditApi.GetRedditPostAsync(subreddit, postId, false, cancellationToken);

            if (!redditResult.IsSuccess)
            {
                return Result<IEmbed>.FromError
                (
                    new BetterEmbedError(message, "Unable to retrieve post data."),
                    redditResult
                );
            }

            IRedditPost redditPost = redditResult.Entity;

            var channelResult = await _channelApi.GetChannelAsync(message.ChannelID, cancellationToken);

            if (!channelResult.IsSuccess)
            {
                return Result<IEmbed>.FromError
                (
                    new BetterEmbedError(message, "Unable to retrieve Discord Channel."),
                    channelResult
                );
            }

            var channel = channelResult.Entity;

            var channelIsNsfw = (channel.IsNsfw.HasValue && channel.IsNsfw.Value) ||
                                (message.GuildID.HasValue && await IsGuildNsfw(message.GuildID.Value, cancellationToken));
            // Is the post flagged NSFW
            if (redditPost.WhitelistStatus.Contains("nsfw") && !channelIsNsfw)
            {
                return Result<IEmbed>.FromError(new NotSafeForWorkError());
            }

            var userResult = await _redditApi.GetRedditUserAsync(redditPost.Author, false, cancellationToken);
            var userIconUrl = string.Empty;

            if (userResult.IsSuccess)
            {
                var user = userResult.Entity;
                var url = new Uri(user.IconImage);
                userIconUrl = $"{url.Host}{url.AbsolutePath}";
            }

            var embedBuilder = new EmbedBuilder()
                .WithTitle(redditPost.Title)
                .WithAuthor(redditPost.Author, userIconUrl)
                .WithFooter($"Posted on {redditPost.Subreddit}",
                    "https://www.redditstatic.com/desktop2x/img/favicon/android-icon-192x192.png")
                .WithTimestamp(redditPost.PostDate)
                .WithColor(Color.DimGray);

            embedBuilder.Provider = new EmbedProvider("Reddit", "https://www.reddit.com");

            embedBuilder.AddField("Score", $"{redditPost.Score} ({redditPost.UpvoteRatio * 100}%)", true); 
            
            if (redditPost.PostFlair.HasValue)
            {
                if (redditPost.PostFlair.Value.Contains(":"))
                {
                    var parts = redditPost.PostFlair.Value.Split(":");
                    embedBuilder.AddField($"{parts[0]}:", parts[1], true);
                }
                else
                {
                    embedBuilder.AddField("Post Flair:", redditPost.PostFlair.Value, true);
                }
            }
            
            // Set up media type
            if (redditPost.PostHint.HasValue)
            {
                embedBuilder.Type = redditPost.PostHint.Value switch
                {
                    "rich:video" => EmbedType.Video,
                    "hosted:video" => EmbedType.Video,
                    "video" => EmbedType.Video,
                    "image" when redditPost.Url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) => EmbedType.GIFV,
                    "image" => EmbedType.Image,
                    "link" => EmbedType.Link,
                    _ => EmbedType.Rich
                };
            }

            switch (embedBuilder.Type)
            {
                case EmbedType.Image:
                case EmbedType.GIFV:
                {
                    embedBuilder.WithImageUrl(redditPost.Url);
                    break;
                }
                case EmbedType.Video when redditPost.Media.HasValue:
                {
                    var media = redditPost.Media.Value;
                    if (media.RedditVideo.HasValue)
                    {
                        var redditVideo = media.RedditVideo.Value;

                        if (redditVideo.IsGif)
                        {
                            embedBuilder.WithImageUrl(redditVideo.Url);
                        }
                        else
                        {
                            embedBuilder.Video = new EmbedVideo
                            (redditVideo.Url,
                                Height: redditVideo.Height,
                                Width: redditVideo.Width
                            );
                        }

                        break;
                    }

                    if (media.ExternalVideo.HasValue)
                    {
                        var oembed = media.ExternalVideo.Value;

                        embedBuilder.Video = new EmbedVideo
                        (redditPost.Url,
                            Height: oembed.Video.Value.Height,
                            Width: oembed.Video.Value.Height
                        );
                    }

                    break;
                }
                case EmbedType.Link:
                case EmbedType.Article:
                {
                    embedBuilder.WithThumbnailUrl(redditPost.Thumbnail);
                    embedBuilder.WithDescription(FormatUtilities.Url(redditPost.Url, redditPost.Url));
                    break;
                }
                case EmbedType.Rich:
                default:
                {
                    embedBuilder.WithDescription(redditPost.Text.Value.Truncate(EmbedConstants.MaxDescriptionLength,
                        $"…\n{FormatUtilities.Url("Read More", embedBuilder.Url)}"));
                    break;
                }
            }

            var embedAssurance = embedBuilder.Ensure();

            return embedAssurance.IsSuccess
                ? embedBuilder.Build()
                : Result<IEmbed>.FromError(embedAssurance);
        }

        private async Task<bool> IsGuildNsfw(Snowflake guildId, CancellationToken cancellationToken = default)
        {
            var guildResult = await _guildApi.GetGuildAsync(guildId, false, cancellationToken);

            if (!guildResult.IsSuccess)
            {
                return false;
            }

            var guild = guildResult.Entity;
            return guild.NSFWLevel == GuildNSFWLevel.Explicit;
        }

        private bool IsChannelNsfw(IChannel channel, CancellationToken cancellationToken = default)
        {
           return channel.IsNsfw.HasValue && channel.IsNsfw.Value;
        }
    }
}
