using System.Text.Json.Serialization;

namespace Gest.Simulador.Dtos;

internal sealed class ParkedWebhook
{
    [JsonPropertyName("license_plate")]
    public string? LicensePlate { get; set; }

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }

    [JsonPropertyName("event_type")]
    public string? EventType { get; set; }
}