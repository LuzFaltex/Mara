using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;

namespace Mara.Plugins.Core.Responders
{
    public sealed class UnknownEventResponder : IResponder<UnknownEvent>
    {
        private readonly ILogger<UnknownEventResponder> _logger;

        public UnknownEventResponder(ILogger<UnknownEventResponder> logger)
        {
            _logger = logger;
        }

        public Task<Result> RespondAsync(UnknownEvent gatewayEvent, CancellationToken ct = new CancellationToken())
        {
            _logger.LogInformation($"Captured unknown event: {gatewayEvent.Data}");
            return Task.FromResult(Result.FromSuccess());
        }
    }
}
