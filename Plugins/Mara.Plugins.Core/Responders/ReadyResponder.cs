using System.Threading;
using System.Threading.Tasks;
using Mara.Common.Discord.Feedback;
using Mara.Common.Events;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.API.Objects;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway;
using Remora.Results;

namespace Mara.Plugins.Core.Responders
{
    public sealed class ReadyResponder : LoggingEventResponderBase<IReady>
    {
        private readonly IDiscordRestOAuth2API _oauthApi;
        private readonly IdentityInformationConfiguration _identityInfo;
        private readonly DiscordGatewayClient _gatewayClient;
        private readonly SlashService _slashService;
        private readonly ILogger<ReadyResponder> _logger;

        public ReadyResponder
        (
            ILogger<ReadyResponder> logger,
            IDiscordRestOAuth2API oauth,
            IdentityInformationConfiguration identityInfo,
            DiscordGatewayClient gatewayClient,
            SlashService slashService 
        ) : base(logger)
        {
            _logger = logger;
            _oauthApi = oauth;
            _identityInfo = identityInfo;
            _gatewayClient = gatewayClient;
            _slashService = slashService;
        }

        public override async Task<Result> HandleAsync(IReady gatewayEvent, CancellationToken cancellationToken = default)
        {
            _identityInfo.Id = gatewayEvent.User.ID;

            var getApplication = await _oauthApi.GetCurrentBotApplicationInformationAsync(cancellationToken);
            if (!getApplication.IsSuccess)
            {
                return Result.FromError(getApplication);
            }

            _identityInfo.Application = getApplication.Entity;

            var application = getApplication.Entity;

            // Set status
            var updatePresence = new UpdatePresence(ClientStatus.Online, false, null,
                new[] {new Activity("anime", ActivityType.Watching)});
            _gatewayClient.SubmitCommand(updatePresence);

            return Result.FromSuccess();
        }
    }
}
