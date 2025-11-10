using System.Text.Json.Serialization;

namespace Gest.Simulador.Dtos;

internal class GarageConfigDto
{
    [JsonPropertyName("garage")]
    public List<GarageSectorDto> Garage { get; set; } = [];
    [JsonPropertyName("spots")]
    public List<GarageSpotDto> Spots { get; set; } = [];
}

internal class GarageSectorDto
{
    [JsonPropertyName("sector")]
    public string Sector { get; set; } = default!;
    [JsonPropertyName("basePrice")]
    public decimal BasePrice { get; set; }
    [JsonPropertyName("max_capacity")]
    public int Max_Capacity { get; set; }
}

internal class GarageSpotDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("sector")]
    public string Sector { get; set; } = default!;
    [JsonPropertyName("lat")]
    public double Lat { get; set; }
    [JsonPropertyName("lng")]
    public double Lng { get; set; }
}

