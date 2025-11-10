using MediatR;
using GEST.Application.Abstractions;

namespace GEST.Application.Services.PublishEvent;

public sealed class MediatorPublishEvent(IMediator mediator) : IPublishEvent
{
    public Task PublishAsync(INotification notification, CancellationToken ct = default)
        => mediator.Publish(notification, ct);
}
