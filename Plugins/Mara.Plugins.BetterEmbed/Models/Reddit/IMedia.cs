using Remora.Discord.Core;

namespace Mara.Plugins.BetterEmbeds.Models.Reddit
{
    public interface IMedia
    {
        Optional<RedditVideo> RedditVideo { get; init; }
        Optional<OEmbed.OEmbed> ExternalVideo { get; init; }
    }
}
