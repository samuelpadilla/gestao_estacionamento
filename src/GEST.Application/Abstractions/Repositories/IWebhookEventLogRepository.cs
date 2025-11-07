using GEST.Domain.Entities;

namespace GEST.Application.Abstractions.Repositories;

public interface IWebhookEventLogRepository
{
    Task AddAsync(WebhookEventLog log, CancellationToken ct);
}
