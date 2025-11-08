namespace GEST.Application.Dtos.Garage;

public sealed class GarageSectorStateDto
{
    public string SectorCode { get; set; }
    public int Capacity { get; set; }
    public int Occupied { get; set; }
    public int Available { get; set; }
}
