using System.Text.Json.Serialization;

namespace Gest.Simulador.Dtos;

internal sealed class ExitWebhook
{
    [JsonPropertyName("license_plate")]
    public string? LicensePlate { get; set; }

    [JsonPropertyName("exit_time")]
    public DateTime ExitTime { get; set; }

    [JsonPropertyName("event_type")]
    public string? EventType { get; set; }
}
