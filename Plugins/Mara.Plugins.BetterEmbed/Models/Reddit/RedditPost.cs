using System;
using Remora.Discord.Core;

namespace Mara.Plugins.BetterEmbeds.Models.Reddit
{
    public record RedditPost
    (
        string Title,
        string Subreddit,
        string Author,
        string Url,
        string Permalink,
        Optional<string> Text,
        int Score,
        double UpvoteRatio,
        DateTime PostDate,
        Optional<string> PostFlair,
        Optional<Media> Media,
        bool IsVideo,
        Optional<string> PostHint,
        string WhitelistStatus,
        string Thumbnail,
        Optional<int?> ThumbnailWidth,
        Optional<int?> ThumbnailHeight
    ) : IRedditPost;
}
