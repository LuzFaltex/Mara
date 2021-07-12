using System;
using System.Text.Json.Serialization;
using Mara.Plugins.BetterEmbeds.Models.Reddit.Converters;
using Remora.Discord.Core;

namespace Mara.Plugins.BetterEmbeds.Models.Reddit
{
    public record RedditPost
    (
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("subreddit_name_prefixed")] string Subreddit,
        [property: JsonPropertyName("author")] string Author,
        [property: JsonPropertyName("url")] string Url,
        [property: JsonPropertyName("permalink")] string Permalink,
        [property: JsonPropertyName("selftext")] Optional<string> Text,
        [property: JsonPropertyName("score")] int Score,
        [property: JsonPropertyName("upvote_ratio")] double UpvoteRatio,
        [property: JsonPropertyName("created"), JsonConverter(typeof(UtcTimestampConverter))] DateTime PostDate,
        [property: JsonPropertyName("link_flair_text")] Optional<string> PostFlair,
        [property: JsonPropertyName("media")] Optional<Media> Media,
        [property: JsonPropertyName("is_video")] bool IsVideo,
        [property: JsonPropertyName("post_hint")] Optional<string> PostHint,
        [property: JsonPropertyName("whitelist_status")] string WhitelistStatus,
        [property: JsonPropertyName("thumbnail")] string Thumbnail,
        [property: JsonPropertyName("thumbnail_width")] int ThumbnailWidth,
        [property: JsonPropertyName("thumbnail_height")] int ThumbnailHeight
    ) : IRedditPost;
}
