using System.Text.Json.Serialization;

namespace Gest.Simulador.Dtos;

internal sealed class EntryWebhook
{
    [JsonPropertyName("license_plate")]
    public string? LicensePlate { get; set; }

    [JsonPropertyName("entry_time")]
    public DateTime EntryTime { get; set; }

    [JsonPropertyName("event_type")]
    public string? EventType { get; set; }
}
