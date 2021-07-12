namespace Mara.Common.Discord.Feedback.Errors
{
    /// <summary>
    /// Returned when a user tries to post Not Safe For Work content in a SFW channel.
    /// </summary>
    public record NotSafeForWorkError(string Message = "Cannot post NSFW message in a channel not marked as NSFW.") : UserError(Message);
}
