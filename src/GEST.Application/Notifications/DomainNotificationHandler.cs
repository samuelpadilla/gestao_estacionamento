using MediatR;

namespace GEST.Application.Notifications;

public sealed class DomainNotificationHandler(
    INotificationContext ctx
    ) : INotificationHandler<DomainNotification>
{
    public Task Handle(DomainNotification notification, CancellationToken cancellationToken)
    {
        ctx.Add(notification.Key, notification.Message);
        return Task.CompletedTask;
    }
}
