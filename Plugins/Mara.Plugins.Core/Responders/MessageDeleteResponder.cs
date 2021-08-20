using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mara.Common.Discord;
using Remora.Discord.API.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Discord.Rest.API;
using Remora.Results;

namespace Mara.Plugins.Core.Responders
{
    public sealed class MessageDeleteResponder : IResponder<MessageReactionAdd>
    {
        private readonly DiscordRestChannelAPI _channelApi;

        public MessageDeleteResponder(DiscordRestChannelAPI channelApi)
        {
            _channelApi = channelApi;
        }

        public async Task<Result> RespondAsync(MessageReactionAdd gatewayEvent, CancellationToken cancellationToken = new CancellationToken())
        {
            // If the reaction wasn't ❌, skip.
            if (!gatewayEvent.Emoji.Name.Equals("❌"))
                return Result.FromSuccess();

            var messageResult = await _channelApi.GetChannelMessageAsync
            (
                gatewayEvent.ChannelID,
                gatewayEvent.MessageID,
                cancellationToken
            );
            
            if (!messageResult.IsSuccess)
                return Result.FromError(messageResult);

            var message = messageResult.Entity!;

            // If the message has no deletable embeds, skip
            if (message.Embeds.All(x => x.Footer != EmbedConstants.DefaultFooter))
            {
                return Result.FromSuccess();
            }

            // At least one embed is deletable. Delete the message.
            return await _channelApi.DeleteMessageAsync
            (
                gatewayEvent.ChannelID,
                gatewayEvent.MessageID,
                "User requested deletion.",
                cancellationToken
            );
        }
    }
}
