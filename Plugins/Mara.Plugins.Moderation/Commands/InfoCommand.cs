using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Mara.Common.Discord;
using Mara.Plugins.Moderation.Services;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Mara.Plugins.Moderation.Commands
{
    public sealed class InfoCommand : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly IDiscordRestUserAPI _userApi;
        private readonly IDiscordRestGuildAPI _guildApi;
        private readonly UserService _userService;
        private readonly ICommandContext _context;

        public InfoCommand
        (
            FeedbackService feedbackService, 
            IDiscordRestUserAPI userApi, 
            IDiscordRestGuildAPI guildApi, 
            UserService userService, 
            ICommandContext context
        )
        {
            _feedbackService = feedbackService;
            _userApi = userApi;
            _guildApi = guildApi;
            _userService = userService;
            _context = context;
        }

        [Command("info")]
        [Description("Returns information about a user.")]
        [CommandType(ApplicationCommandType.ChatInput)]
        public async Task<IResult> ShowUserInfoChatAsync(IUser user, Snowflake? guildId = null)
        {
            var buildEmbedResult = await BuildUserInfoEmbed(user, guildId);

            if (!buildEmbedResult.IsSuccess)
            {
                return buildEmbedResult;
            }

            return await _feedbackService.SendContextualEmbedAsync(buildEmbedResult.Entity, ct: base.CancellationToken);
        }

        [Command("User Info")]
        [Description("Returns information about a user.")]
        [CommandType(ApplicationCommandType.User)]
        public async Task<IResult> ShowUserInfoMenuAsync()
        {
            var user = _context.User;

            var buildEmbedResult = _context.GuildID.HasValue
                ? await BuildUserInfoEmbed(user, _context.GuildID.Value)
                : await BuildUserInfoEmbed(user, null);

            if (!buildEmbedResult.IsSuccess)
            {
                return buildEmbedResult;
            }

            return await _feedbackService.SendContextualEmbedAsync(buildEmbedResult.Entity, ct: base.CancellationToken);
        }

        private async Task<Result<Embed>> BuildUserInfoEmbed(IUser user, Snowflake? guildId)
        {
            var userInfo = await _userService.GetUserInformation(user);

            if (!userInfo.IsSuccess)
            {
                return Result<Embed>.FromError(userInfo);
            }

            var embedBuilder = new EmbedBuilder()
                .WithUserAsAuthor(user);

            embedBuilder.WithThumbnailUrl(embedBuilder.Author?.IconUrl.Value ?? "");

            var userInformation = new StringBuilder()
                .AppendLine($"ID: {user.ID.Value}")
                .AppendLine($"Profile: {FormatUtilities.Mention(user)}")
                .AppendLine($"First Seen: {userInfo.Entity.FirstSeen}")
                .AppendLine($"Last Seen: {userInfo.Entity.LastSeen}");
            embedBuilder.AddField("❯ User Information", userInformation.ToString());

            var memberInfo = new StringBuilder()
                .AppendLine($"Created: {user.ID.Timestamp}");

            Result<DateTimeOffset> joinDate = default;

            if (guildId is not null)
            {
                joinDate = await GetJoinDate(user, guildId.Value);
            }

            if (_context.GuildID.HasValue)
            {
                joinDate = await GetJoinDate(user, _context.GuildID.Value);
            }

            if (joinDate.IsSuccess)
            {
                memberInfo.AppendLine($"Joined: {FormatUtilities.DynamicTimeStamp(joinDate.Entity, FormatUtilities.TimeStampStyle.RelativeTime)}");
                FormatUtilities.DynamicTimeStamp(joinDate.Entity, FormatUtilities.TimeStampStyle.ShortTime);
            }

            embedBuilder.WithFooter(EmbedConstants.DefaultFooter);
            embedBuilder.WithCurrentTimestamp();

            var ensure = embedBuilder.Ensure();

            if (!ensure.IsSuccess)
            {
                return Result<Embed>.FromError(ensure);
            }

            return embedBuilder.Build();
        }

        private async Task<Result<DateTimeOffset>> GetJoinDate(IUser user, Snowflake guildId)
        {
            var guildMember = await _guildApi.GetGuildMemberAsync(guildId, user.ID, base.CancellationToken);

            return guildMember.IsSuccess
                ? guildMember.Entity.JoinedAt
                : Result<DateTimeOffset>.FromError(guildMember);
        }
    }
}
