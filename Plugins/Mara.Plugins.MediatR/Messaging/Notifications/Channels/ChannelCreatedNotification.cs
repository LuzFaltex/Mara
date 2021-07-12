using MediatR;
using Remora.Discord.API.Abstractions.Gateway.Events;

namespace Mara.Plugins.Mediator.Messaging.Notifications.Channels
{
    public record ChannelCreatedNotification(IChannelCreate ChannelCreated) : INotification;
}
