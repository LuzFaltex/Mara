using System;
using System.Collections.Generic;
using Remora.Rest.Core;

namespace Mara.Plugins.Moderation.Models
{

    public sealed class UserInformation
    {
        public Snowflake Id { get; init; }

        public DateTime FirstSeen { get; init; }

        public DateTime LastSeen { get; set; }

        public List<Infraction> Infractions { get; set; } = new();
    }
}
