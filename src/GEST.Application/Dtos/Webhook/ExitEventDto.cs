namespace GEST.Application.Dtos.Webhook;

public sealed class ExitEventDto : WebhookEventDto
{
    public DateTime Exit_Time { get; set; } // ISO UTC
}
