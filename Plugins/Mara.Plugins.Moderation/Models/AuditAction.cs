namespace Mara.Plugins.Moderation.Models
{
    /// <summary>
    /// Represents an action taken during an audit, such as a channel name change or the user that was banned.
    /// </summary>
    public sealed class AuditAction
    {
        /// <summary>
        /// The unique identifier for this item.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// A reference back to the audit this action belongs to.
        /// </summary>
        public Audit Audit { get; set; } = new();
    }
}
