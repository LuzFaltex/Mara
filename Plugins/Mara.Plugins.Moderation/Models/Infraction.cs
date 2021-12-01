using Remora.Rest.Core;
using System;

namespace Mara.Plugins.Moderation.Models
{
    public sealed class Infraction
    {
        /// <summary>
        /// The unique identifier of this infraction.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// A reference back to the user that has this infraction.
        /// </summary>
        public UserInformation User { get; set; } = new();

        /// <summary>
        /// The date and time the infraction took place.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// What kind of action was taken by the responsible moderator.
        /// </summary>
        public InfractionKind InfractionKind { get; set; }

        /// <summary>
        /// The user responsible for resolving the issue.
        /// </summary>
        public Snowflake ResponsibleModerator { get; set; }

        /// <summary>
        /// Information about this infraction.
        /// </summary>
        public string Reason { get; set; } = "";

        /// <summary>
        /// <c>True</c> if this infraction was rescinded; otherwise, <c>False</c>.
        /// </summary>
        public bool Rescinded { get; set; }
    }

    public enum InfractionKind
    {
        Warn = 1,
        Mute = 2,
        Kick = 4,
        SoftBan = 8,
        Ban = 16
    }
}
