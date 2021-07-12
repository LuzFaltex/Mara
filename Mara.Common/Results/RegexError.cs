using Remora.Results;

namespace Mara.Common.Results
{
    /// <summary>
    /// Represents an error which occurred when parsing a regular expression.
    /// </summary>
    public record RegexError
    (
        string Value,
        string Reason = "No reason provided."
    )
    : ResultError($"Regex failed on string \"{Value}\".");
}
