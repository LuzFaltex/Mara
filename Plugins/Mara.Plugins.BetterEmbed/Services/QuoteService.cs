using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Mara.Common.Discord;
using Mara.Common.Extensions;
using Mara.Plugins.BetterEmbeds.Results;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Core;
using Remora.Results;

namespace Mara.Plugins.BetterEmbeds.Services
{
    public sealed class QuoteService : IQuoteService
    {
        private readonly IDiscordRestChannelAPI _channelApi;

        public QuoteService(IDiscordRestChannelAPI channelApi)
        {
            _channelApi = channelApi;
        }

        public async Task<Result<IEmbed>> BuildQuoteEmbedAsync(IMessage message, IUser executingUser)
        {
            if (IsQuote(message))
            {
                return new BetterEmbedError(message, "The quoted message is already a bot quote!");
            }

            var fields = new List<IEmbedField>(EmbedConstants.MaxFieldCount);

            var jumpUrl = await message.GetJumpUrlAsync(_channelApi);
            if (!jumpUrl.IsSuccess)
            {
                return Result<IEmbed>.FromError(jumpUrl);
            }

            var firstEmbed = message.Embeds.Any()
                ? new Optional<IEmbed>(message.Embeds[0])
                : new Optional<IEmbed>();

            if (firstEmbed.HasValue)
            {
                if (firstEmbed.Value.Fields.HasValue)
                {
                    fields.AddRange(firstEmbed.Value.Fields.Value);
                }

                if (fields.Count < fields.Capacity)
                {
                    fields.Add(new EmbedField("Quoted by", $"{FormatUtilities.Mention(executingUser)} from {FormatUtilities.Bold(jumpUrl.Entity)}", false));
                }

                var embed = (Embed) firstEmbed.Value;

                return embed.Type == EmbedType.Rich
                    ? embed with {Fields = fields}
                    : embed with
                    {
                        Description = message.Content,
                        Timestamp = message.Timestamp,
                        Colour = EmbedConstants.DefaultColor,
                        Footer = EmbedConstants.DefaultFooter
                    };
            }

            if (message.Activity.HasValue)
            {
                fields.Add(new EmbedField("Invite Type", message.Activity.Value.Type.ToString().Humanize()));
                fields.Add(new EmbedField("Party Id", message.Activity.Value.PartyID.HasValue ? message.Activity.Value.PartyID.Value : "No party id."));
            }

            return new EmbedBuilder()
                .WithDescription(message.Content)
                .WithTimestamp(message.Timestamp)
                .WithUserAsAuthor(message.Author)
                .SetFields(fields)
                .Build();
        }

        private static bool IsQuote(IMessage message)
            => message.Embeds.Any(
                embed => embed.Fields.HasValue && 
                         embed.Fields.Value.Any(field => field.Name == "Quoted by"));
    }

}
