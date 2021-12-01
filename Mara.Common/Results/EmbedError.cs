using Remora.Results;

namespace Mara.Common.Results
{
    /// <summary>
    /// Represents an error which occurs when building an Embed.
    /// </summary>
    public record EmbedError(string Reason) : ResultError($"Failed to create an embed: {Reason}");
}
