using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mara.Plugins.Moderation.Models
{
    public enum EventType
    {
        /// <summary>
        /// Placeholder. Used for when there is no valid event type.
        /// </summary>
        None = 0,

        ChannelCreate = 1,
        ChannelDelete = 2,
        ChannelUpdate = 3,
        ChannelPinsUpdate = 4,
        StageInstanceCreate = 5,
        StageInstanceDelete = 6,
        StageInstanceUpdate = 7,
        ThreadCreate = 8,
        ThreadDelete = 9,
        ThreadUpdate = 10,

        GuildBanAdd = 11,
        GuildBanRemove = 12,
        GuildCreate = 13,
        GuildDelete = 14,
        GuildEmojisUpdate = 15,
        GuildIntegrationsUpdate = 16,
        GuildMemberAdd = 17,
        GuildMemberRemove = 18,
        GuildMemberUpdate = 19,
        GuildRoleCreate = 20,
        GuildRoleDelete = 21,
        GuildRoleUpdate = 22,
        GuildStickersUpdate = 23,
        GuildUpdate = 24,
        GuildInviteCreate = 25,
        GuildInviteDelete = 26,

        MessageCreate = 27,
        MessageDelete = 28,
        MessageUpdate = 29,
        MessageReactionAdd = 30,
        MessageReactionRemove = 31,

        WebhookUpdate = 32        
    }
}
