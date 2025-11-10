using MediatR;

namespace GEST.Application.Notifications;

public sealed record DomainNotification(string Key, string Message) : INotification;
