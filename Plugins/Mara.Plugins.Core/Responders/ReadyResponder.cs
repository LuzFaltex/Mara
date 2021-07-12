using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mara.Common.Discord.Feedback;
using Mara.Common.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Gateway.Commands;
using Remora.Discord.API.Objects;
using Remora.Discord.Gateway;
using Remora.Results;

namespace Mara.Plugins.Core.Responders
{
    public sealed class ReadyResponder : LoggingEventResponderBase<IReady>
    {
        private readonly IDiscordRestOAuth2API _oauthApi;
        private readonly IdentityInformationConfiguration _identityInfo;
        private readonly DiscordGatewayClient _gatewayClient;

        public ReadyResponder
        (
            ILogger<ReadyResponder> logger,
            IDiscordRestOAuth2API oauth,
            IdentityInformationConfiguration identity,
            DiscordGatewayClient gatewayClient
        ) : base(logger)
        {
            _oauthApi = oauth;
            _identityInfo = identity;
            _gatewayClient = gatewayClient;
        }

        public override async Task<Result> HandleAsync(IReady gatewayEvent, CancellationToken cancellationToken = default)
        {
            _identityInfo.Id = gatewayEvent.User.ID;

            var getApplication = await _oauthApi.GetCurrentBotApplicationInformationAsync(cancellationToken);
            if (!getApplication.IsSuccess)
            {
                return Result.FromError(getApplication);
            }

            var application = getApplication.Entity;

            _identityInfo.ApplicationId = application.ID;
            _identityInfo.OwnerId = application.Owner.ID.Value;

            // Set status
            var updatePresence = new UpdatePresence(ClientStatus.Online, false, null,
                new[] {new Activity("anime", ActivityType.Watching)});
            _gatewayClient.SubmitCommandAsync(updatePresence);

            return Result.FromSuccess();
        }
    }
}
