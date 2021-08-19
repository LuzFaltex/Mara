using System;
using Remora.Discord.Core;

namespace Mara.Plugins.BetterEmbeds.Models.Reddit
{
    public interface IRedditPost
    {
        public string Title { get; }
        public string Subreddit { get; }
        public string Author { get; }
        public string Url { get; }
        public string Permalink { get; }
        public Optional<string> Text { get; }
        public int Score { get; }
        public double UpvoteRatio { get; }
        public DateTime PostDate { get; }
        public Optional<string> PostFlair { get; }
        public Optional<Media> Media { get; }
        public bool IsVideo { get; }
        /// <summary>
        /// Not available or null - text
        /// rich:video - get oembed
        /// hosted:video - v.reddit video
        /// </summary>
        public Optional<string> PostHint { get; }
        public string WhitelistStatus { get; }
        public string Thumbnail { get; }
        public Optional<int?> ThumbnailWidth { get; }
        public Optional<int?> ThumbnailHeight { get; }
    }
}
