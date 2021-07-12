using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Mara.Plugins.Mediator.Messaging
{
    public abstract class LoggingNotificationHandlerBase<TNotification> : INotificationHandler<TNotification>
        where TNotification : INotification
    {
        private readonly ILogger<LoggingNotificationHandlerBase<TNotification>> _logger;
        private static readonly string NotificationTypeName = typeof(TNotification).Name;

        protected LoggingNotificationHandlerBase(ILogger<LoggingNotificationHandlerBase<TNotification>> logger)
        {
            _logger = logger;
        }
        public async Task Handle(TNotification notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {Request}", NotificationTypeName);

            var sw = Stopwatch.StartNew();
            await HandleAsync(notification, cancellationToken);
            sw.Stop();

            _logger.LogInformation("Handled {Response} in {Elapsed}", NotificationTypeName, sw.Elapsed.Humanize(precision: 5));
        }

        public abstract ValueTask HandleAsync(TNotification request, CancellationToken cancellationToken);
    }
}
