using Remora.Results;

namespace Mara.Common.Discord.Feedback.Errors
{
    /// <summary>
    /// Represents an error on the users part which should be relayed to them.
    /// </summary>
    public record UserError(string Message) : ResultError(Message);
}
