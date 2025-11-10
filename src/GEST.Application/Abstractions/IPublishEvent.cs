using MediatR;

namespace GEST.Application.Abstractions;

// Abstração fina para publicar eventos/notifications (facilita teste)
public interface IPublishEvent
{
    Task PublishAsync(INotification notification, CancellationToken ct = default);
}
