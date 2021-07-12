using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Logging;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Responders;
using Remora.Results;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Mara.Common.Events
{
    public abstract class LoggingEventResponderBase<TEvent> : IResponder<TEvent>
        where TEvent : IGatewayEvent
    {
        private readonly ILogger<LoggingEventResponderBase<TEvent>> _logger;
        private static readonly string EventTypeName = typeof(TEvent).Name;

        protected LoggingEventResponderBase(ILogger<LoggingEventResponderBase<TEvent>> logger)
        {
            _logger = logger;
        }

        public async Task<Result> RespondAsync(TEvent gatewayEvent, CancellationToken ct = default)
        {
            _logger.LogInformation("Handling {Event}", EventTypeName);

            var sw = Stopwatch.StartNew();
            var response = await HandleAsync(gatewayEvent, ct);
            sw.Stop();

            _logger.LogInformation("Handled {Response} in {Elapsed}", EventTypeName, sw.Elapsed.Humanize(precision: 5));
            return response;
        }

        public abstract Task<Result> HandleAsync(TEvent gatewayEvent, CancellationToken cancellationToken = default);
    }
}
