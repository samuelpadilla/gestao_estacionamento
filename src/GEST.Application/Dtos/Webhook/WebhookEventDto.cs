namespace GEST.Application.Dtos.Webhook;

public abstract class WebhookEventDto
{
    public string Event_Type { get; set; } = default!;
    public string License_Plate { get; set; } = default!;
}
