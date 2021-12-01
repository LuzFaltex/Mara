using Remora.Rest.Core;
using System.Collections.Generic;

namespace Mara.Plugins.Moderation.Models
{
    /// <summary>
    /// Represents an action that takes place on a server.
    /// </summary>
    public sealed class Audit
    {
        /// <summary>
        /// The audit's unique id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The id of the Guild where this event took place.
        /// </summary>
        public Snowflake GuildId { get; set; }

        /// <summary>
        /// The user or bot responsible for the action.
        /// </summary>
        public Snowflake Source { get; set; }

        /// <summary>
        /// The kind of event that took place
        /// </summary>
        public EventType EventType { get; set; }

        /// <summary>
        /// The target of the action
        /// </summary>
        public Snowflake Target { get; set; }

        /// <summary>
        /// A collection of actions taken during this change.
        /// </summary>
        public List<AuditAction> AuditActions { get; set; } = new();

        /// <summary>
        /// If the action was performed as part of a change, its change number goes here.
        /// </summary>
        public int? ChangeNumber { get; set; }

        /// <summary>
        /// User-provided information regarding the audit entry.
        /// </summary>
        public string? Comment { get; set; }
    }
}
