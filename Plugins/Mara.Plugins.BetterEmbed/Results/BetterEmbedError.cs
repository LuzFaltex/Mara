using Remora.Discord.API.Abstractions.Objects;
using Remora.Results;

namespace Mara.Plugins.BetterEmbeds.Results
{
    public record BetterEmbedError(IMessage DiscordMessage, string Reason) : ResultError(
            $"Could not generate a better embed for message {DiscordMessage.ID}.");
}
