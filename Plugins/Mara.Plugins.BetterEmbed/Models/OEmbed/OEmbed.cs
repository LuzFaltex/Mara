using System;
using Remora.Discord.Core;

namespace Mara.Plugins.BetterEmbeds.Models.OEmbed
{
    public record OEmbed
    (
        string Type,
        Version Version,
        Optional<string> Title,
        Optional<string> AuthorName,
        Optional<string> AuthorUrl,
        Optional<string> ProviderName,
        Optional<string> ProviderUrl,
        Optional<int> CacheAge,
        Optional<string> ThumbnailUrl,
        Optional<int> ThumbnailWidth,
        Optional<int> ThumbnailHeight,
        Optional<IPhoto> Photo,
        Optional<IVideo> Video
    ) : IOEmbed;
}
