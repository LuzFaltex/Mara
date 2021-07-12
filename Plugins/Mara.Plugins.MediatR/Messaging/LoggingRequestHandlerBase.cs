using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Humanizer;
using MediatR;

using Stopwatch = System.Diagnostics.Stopwatch;

namespace Mara.Plugins.Mediator.Messaging
{
    public abstract class LoggingRequestHandlerBase<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingRequestHandlerBase<TRequest, TResponse>> _logger;
        private static readonly string RequestTypeName = typeof(TRequest).Name;

        protected LoggingRequestHandlerBase(ILogger<LoggingRequestHandlerBase<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {Request}", RequestTypeName);

            var sw = Stopwatch.StartNew();
            var response = await HandleAsync(request, cancellationToken);
            sw.Stop();

            _logger.LogInformation("Handled {Response} in {Elapsed}", RequestTypeName, sw.Elapsed.Humanize(precision: 5));
            return response;
        }

        public abstract ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
