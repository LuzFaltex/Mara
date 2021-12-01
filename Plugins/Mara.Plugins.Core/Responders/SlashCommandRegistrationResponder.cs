using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Commands.Services;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;

using DiscordConstants = Remora.Discord.API.Constants;

namespace Mara.Plugins.Core.Responders
{
    public sealed class SlashCommandRegistrationResponder : IResponder<IGuildCreate>
    {
        private readonly SlashService _slashService;
        private readonly ILogger<SlashCommandRegistrationResponder> _logger;

        public SlashCommandRegistrationResponder(SlashService slashService, ILogger<SlashCommandRegistrationResponder> logger)
        {
            _slashService = slashService;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Result> RespondAsync(IGuildCreate gatewayEvent, CancellationToken cancellationToken = new CancellationToken())
        {
            // For debug only
            var guildId = new Snowflake(861515006067998731, DiscordConstants.DiscordEpoch);

            if (gatewayEvent.ID != guildId)
            {
                return Result.FromSuccess();
            }
            
            // Load slash commands
            var checkSlashService = _slashService.SupportsSlashCommands();

            if (checkSlashService.IsSuccess)
            {
                var updateSlash = await _slashService.UpdateSlashCommandsAsync(guildId, cancellationToken);
                if (!updateSlash.IsSuccess)
                {
                    _logger.LogWarning("Failed to update slash commands: {Reason}",
                        updateSlash.Error.Message);

                    return updateSlash;
                }
            }
            else
            {
                _logger.LogWarning("The registered commands of the bot don't support slash commands: {Reason}", checkSlashService.Error.Message);
                return checkSlashService;
            }

            return Result.FromSuccess();
        }
    }
}
