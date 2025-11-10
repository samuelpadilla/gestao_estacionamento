using System.Text.Json.Serialization;

namespace GEST.Application.Dtos.Garage;

public sealed class GarageSpotDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("sector")]
    public string SectorCode { get; set; } = default!;

    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lng")]
    public double Lng { get; set; }
}