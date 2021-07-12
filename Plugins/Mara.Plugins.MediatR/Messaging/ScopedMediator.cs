using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Mara.Plugins.Mediator.Messaging
{
    public sealed class ScopedMediator : IMediator
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public ScopedMediator(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = new CancellationToken())
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IMediator>();
            return await service.Send(request, cancellationToken);
        }

        public async Task<object?> Send(object request, CancellationToken cancellationToken = new CancellationToken())
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IMediator>();
            return await service.Send(request, cancellationToken);
        }

        public async Task Publish(object notification, CancellationToken cancellationToken = new CancellationToken())
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IMediator>();
            await service.Publish(notification, cancellationToken);
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new CancellationToken())
            where TNotification : INotification
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IMediator>();
            await service.Publish(notification, cancellationToken);
        }
    }
}
