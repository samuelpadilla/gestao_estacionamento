using GEST.Application.Abstractions.Repositories;
using GEST.Domain.Entities;

namespace GEST.Infrastructure.Persistence.Repositories;

public sealed class WebhookEventLogRepository(GestDbContext db) : IWebhookEventLogRepository
{
    private readonly GestDbContext _db = db;

    public Task AddAsync(WebhookEventLog log, CancellationToken ct)
    {
        _db.WebhookEventLogs.Add(log);
        return Task.CompletedTask;
    }
}
