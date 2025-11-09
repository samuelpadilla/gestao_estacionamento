namespace GEST.Application.Dtos.Garage;

public sealed class GarageSpotDto
{
    public int Id { get; set; }
    public string SectorCode { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
}