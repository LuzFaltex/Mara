using System.Text.Json.Serialization;
using Remora.Discord.Core;

namespace Mara.Plugins.BetterEmbeds.Models.Reddit
{
    public record Media
    (
        [property: JsonPropertyName("reddit_video")] Optional<RedditVideo> RedditVideo,
        [property: JsonPropertyName("oembed")] Optional<OEmbed.OEmbed> ExternalVideo
    );
}
