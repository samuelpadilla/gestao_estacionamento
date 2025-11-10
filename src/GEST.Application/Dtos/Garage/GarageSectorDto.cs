using System.Text.Json.Serialization;

namespace GEST.Application.Dtos.Garage;

public sealed class GarageSectorDto
{
    [JsonPropertyName("sector")]
    public string Sector { get; set; } = default!;

    [JsonPropertyName("basePrice")]
    public decimal BasePrice { get; set; }

    [JsonPropertyName("max_capacity")]
    public int Max_Capacity { get; set; }
}
