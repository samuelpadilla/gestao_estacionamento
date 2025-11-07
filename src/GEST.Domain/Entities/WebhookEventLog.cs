using GEST.Domain.Enums;

namespace GEST.Domain.Entities;

public class WebhookEventLog
{
    public long Id { get; set; }
    public WebhookEventType EventType { get; set; }

    public string LicensePlate { get; set; } = default!;
    public DateTime ReceivedAtUtc { get; set; }

    // Campos opcionais conforme payload
    public DateTime? EntryTimeUtc { get; set; }
    public DateTime? ExitTimeUtc { get; set; }
    public double? Lat { get; set; }
    public double? Lng { get; set; }

    // Guarda o JSON bruto para auditoria
    public string RawPayloadJson { get; set; } = default!;
}