using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Mara.Plugins.Mediator.Messaging
{
    public static class MessagingSetup
    {
        public static IServiceCollection AddMessagingScoped<TEntryPoint, TMediator>(this IServiceCollection services)
            where TEntryPoint : class
            where TMediator : IMediator
            => services.AddMediatR(cfg => cfg.Using<TMediator>().AsScoped(), typeof(TEntryPoint));
    }
}
