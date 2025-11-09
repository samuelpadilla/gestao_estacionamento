using GEST.Application.Abstractions.Repositories;
using GEST.Domain.Entities;

namespace GEST.Infrastructure.Persistence.Repositories;

public sealed class WebhookEventLogRepository(
    GestDbContext db
    ) : BaseRepository<WebhookEventLog>(db), IWebhookEventLogRepository
{
}
