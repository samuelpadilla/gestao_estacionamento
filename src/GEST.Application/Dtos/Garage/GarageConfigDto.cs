namespace GEST.Application.Dtos.Garage;

public sealed class GarageConfigDto
{
    public List<GarageSectorDto> Garage { get; set; } = [];
    public List<GarageSpotDto> Spots { get; set; } = [];
}
