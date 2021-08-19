using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mara.Common.Discord.Feedback;
using Mara.Common.Events;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Services;
using Remora.Discord.Core;
using Remora.Discord.Gateway;
using Remora.Results;

namespace Mara.Plugins.Core.Responders
{
    public sealed class ReadyResponder : LoggingEventResponderBase<IReady>
    {
        private readonly IDiscordRestOAuth2API _oauthApi;
        private readonly IMemoryCache _cache;
        private readonly DiscordGatewayClient _gatewayClient;
        private readonly SlashService _slashService;
        private readonly ILogger<ReadyResponder> _logger;

        public ReadyResponder
        (
            ILogger<ReadyResponder> logger,
            IDiscordRestOAuth2API oauth,
            IMemoryCache cache,
            DiscordGatewayClient gatewayClient,
            SlashService slashService 
        ) : base(logger)
        {
            _logger = logger;
            _oauthApi = oauth;
            _cache = cache;
            _gatewayClient = gatewayClient;
            _slashService = slashService;
        }

        public override async Task<Result> HandleAsync(IReady gatewayEvent, CancellationToken cancellationToken = default)
        {
            var identityInfo = new IdentityInformationConfiguration
            {
                Id = gatewayEvent.User.ID
            };

            var getApplication = await _oauthApi.GetCurrentBotApplicationInformationAsync(cancellationToken);
            if (!getApplication.IsSuccess)
            {
                return Result.FromError(getApplication);
            }

            var application = getApplication.Entity;

            identityInfo.ApplicationId = application.ID;
            identityInfo.OwnerId = application.Owner.ID.Value;

            // Update memory cache
            _cache.Set(nameof(IdentityInformationConfiguration), identityInfo);

            // Set status
            var updatePresence = new UpdatePresence(ClientStatus.Online, false, null,
                new[] {new Activity("anime", ActivityType.Watching)});
            _gatewayClient.SubmitCommandAsync(updatePresence);

            // Load slash commands
            /*
            var checkSlashService = _slashService.SupportsSlashCommands();

            if (checkSlashService.IsSuccess)
            {
                var updateSlash = await _slashService.UpdateSlashCommandsAsync(ct: cancellationToken);
                if (!updateSlash.IsSuccess)
                {
                    _logger.LogWarning("Failed to update slash commands: {Reason}",
                        updateSlash.Error.Message);
                }
            }
            else
            {
                _logger.LogWarning("The registered commands of the bot don't support slash commands: {Reason}", checkSlashService.Error.Message);
            }
            */

            return Result.FromSuccess();
        }
    }
}
