using System;
using Remora.Discord.Core;

namespace Mara.Plugins.BetterEmbeds.Models.OEmbed
{
    public interface IOEmbed
    {
        string Type { get; init; }
        Version Version { get; init; }
        Optional<string> Title { get; init; }
        Optional<string> AuthorName { get; init; }
        Optional<string> AuthorUrl { get; init; }
        Optional<string> ProviderName { get; init; }
        Optional<string> ProviderUrl { get; init; }
        Optional<int> CacheAge { get; init; }
        Optional<string> ThumbnailUrl { get; init; }
        Optional<int> ThumbnailWidth { get; init; }
        Optional<int> ThumbnailHeight { get; init; }
        Optional<IPhoto> Photo { get; init; }
        Optional<IVideo> Video { get; init; }
    }
}
