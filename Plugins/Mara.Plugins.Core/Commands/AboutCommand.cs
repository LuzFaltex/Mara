using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using Mara.Common.Discord;
using Mara.Common.Discord.Feedback;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Mara.Plugins.Core.Commands
{
    public sealed class AboutCommand : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly IdentityInformationConfiguration _identityInformation;
        private readonly IDiscordRestUserAPI _userApi;

        public AboutCommand(FeedbackService feedbackService, IdentityInformationConfiguration identityInformation, IDiscordRestUserAPI userApi)
        {
            _feedbackService = feedbackService;
            _identityInformation = identityInformation;
            _userApi = userApi;
        }

        [Command("about")]
        [Description("Provides information about the bot.")]
        public async Task<IResult> ShowBotInfoAsync()
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Mara")
                .WithUrl("https://mara.luzfaltex.com")
                .WithColor(Color.Pink)
                .WithDescription("A custom-tailored Discord moderation bot by LuzFaltex.");

            if (_identityInformation.Application.Team is { } team)
            {
                var avatarUrlResult = CDN.GetTeamIconUrl(team, imageSize: 256);

                if (avatarUrlResult.IsSuccess)
                {
                    var avatarUrl = avatarUrlResult.Entity;
                    embedBuilder = embedBuilder.WithAuthor(team.Name, iconUrl: avatarUrl!.AbsoluteUri);
                }
                else
                {
                    var teamOwner = await _userApi.GetUserAsync(team.OwnerUserID, CancellationToken);
                    if (teamOwner.IsSuccess)
                    {
                        embedBuilder = embedBuilder.WithUserAsAuthor(teamOwner.Entity);
                    }
                }
            }
            else
            {
                var ownerId = _identityInformation.Application.Owner!.ID.Value;
                var user = await _userApi.GetUserAsync(ownerId, CancellationToken);
                if (user.IsSuccess)
                {
                    embedBuilder = embedBuilder.WithUserAsAuthor(user.Entity);
                }
            }

            embedBuilder.AddField("Version", typeof(CorePlugin).Assembly.GetName().Version?.ToString(3) ?? "1.0.0");

            return await _feedbackService.SendContextualEmbedAsync(embedBuilder.Build(), ct: CancellationToken);
        }
    }
}
