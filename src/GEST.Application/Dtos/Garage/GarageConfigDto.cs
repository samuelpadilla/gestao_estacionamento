namespace GEST.Application.Dtos.Garage;

public sealed class GarageConfigDto
{
    public List<GarageSectorDto> Garage { get; set; } = new();
    public List<GarageSpotDto> Spots { get; set; } = new();
}
