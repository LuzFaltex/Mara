using System.Drawing;

namespace Mara.Common.Discord.Feedback
{
    /// <summary>
    /// Acts as a base class for user-facing messages.
    /// </summary>
    public abstract record UserMessage(string Message, Color Color);

    /// <summary>
    /// Represents a confirmation message.
    /// </summary>
    public record ConfirmationMessage(string Message) : UserMessage(Message, Color.Cyan);

    /// <summary>
    /// Represents an error message
    /// </summary>
    public record ErrorMessage(string Message) : UserMessage(Message, Color.Red);

    /// <summary>
    /// Represents a warning message.
    /// </summary>
    public record WarningMessage(string Message) : UserMessage(Message, Color.OrangeRed);

    /// <summary>
    /// Represents an informational message.
    /// </summary>
    public record InfoMessage(string Message) : UserMessage(Message, EmbedConstants.DefaultColor);

    /// <summary>
    /// Represents a question or prompt.
    /// </summary>
    public record PromptMessage(string Message) : UserMessage(Message, Color.MediumPurple);
}
