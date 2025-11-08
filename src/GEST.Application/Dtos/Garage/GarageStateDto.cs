namespace GEST.Application.Dtos.Garage;

public sealed class GarageStateDto
{
    public List<GarageSectorStateDto> Sectors { get; set; } = [];
}
