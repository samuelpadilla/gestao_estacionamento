namespace GEST.Application.Dtos.Webhook;

public sealed class ParkedEventDto : WebhookEventDto
{
    public double Lat { get; set; }
    public double Lng { get; set; }
}
