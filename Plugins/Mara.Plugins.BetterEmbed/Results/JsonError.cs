using Remora.Results;

namespace Mara.Plugins.BetterEmbeds.Results
{
    /// <summary>
    /// Represents an error that occurred while parsing JSON.
    /// </summary>
    /// <param name="Message">The error messge.</param>
    public sealed record JsonError(string Message) : ResultError(Message);
}
