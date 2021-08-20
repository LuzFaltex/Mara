using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mara.Common.Discord;
using Mara.Common.Discord.Feedback;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Commands.Services;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Discord.Commands.Responders;
using Remora.Discord.Commands.Services;
using Remora.Discord.Core;
using Remora.Results;

namespace Mara.Plugins.Core.Commands
{
    public sealed class AboutCommand : CommandGroup
    {
        private readonly FeedbackService _feedbackService;
        private readonly IMemoryCache _cache;
        private readonly IDiscordRestUserAPI _userApi;

        public AboutCommand(FeedbackService feedbackService, IMemoryCache cache, IDiscordRestUserAPI userApi)
        {
            _feedbackService = feedbackService;
            _cache = cache;
            _userApi = userApi;
        }

        [Command("about")]
        [Description("Provides information about the bot.")]
        public async Task<IResult> ShowBotInfoAsync()
        {
            var identity = _cache.Get<IdentityInformationConfiguration>(nameof(IdentityInformationConfiguration));
            var user = await _userApi.GetUserAsync(identity.OwnerId);

            var embedBuilder = new EmbedBuilder()
                .WithTitle("Mara")
                .WithUrl("https://mara.luzfaltex.com")
                .WithColor(Color.Pink)
                .WithDescription("A custom-tailored Discord moderation bot by LuzFaltex.");

            if (user.IsSuccess)
            {
                embedBuilder = embedBuilder.WithUserAsAuthor(user.Entity);
            }

            embedBuilder.AddField("Version", typeof(CorePlugin).Assembly.GetName().Version?.ToString(3) ?? "1.0.0");

            return await _feedbackService.SendContextualEmbedAsync(embedBuilder.Build(), this.CancellationToken);
        }
    }
}
