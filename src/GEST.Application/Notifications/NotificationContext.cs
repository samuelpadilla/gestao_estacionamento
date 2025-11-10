namespace GEST.Application.Notifications;

public interface INotificationContext
{
    void Add(string key, string message);
    bool HasNotifications();
    IReadOnlyCollection<(string Key, string Message)> GetAll();
}

public sealed class NotificationContext : INotificationContext
{
    private readonly List<(string Key, string Message)> _notifications = [];

    public void Add(string key, string message) => _notifications.Add((key, message));

    public bool HasNotifications() => _notifications.Count != 0;

    public IReadOnlyCollection<(string Key, string Message)> GetAll() => _notifications.AsReadOnly();
}
