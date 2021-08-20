using Remora.Discord.Core;

namespace Mara.Plugins.BetterEmbeds.Models.Reddit
{
    public record Media
    (
        Optional<RedditVideo> RedditVideo,
        Optional<OEmbed.OEmbed> ExternalVideo
    ) : IMedia;
}
