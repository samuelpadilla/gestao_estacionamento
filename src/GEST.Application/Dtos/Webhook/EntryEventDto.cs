namespace GEST.Application.Dtos.Webhook;

public sealed class EntryEventDto : WebhookEventDto
{
    public DateTime Entry_Time { get; set; } // ISO UTC
}
