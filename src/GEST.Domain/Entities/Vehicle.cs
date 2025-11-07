namespace GEST.Domain.Entities;

public class Vehicle
{
    public string LicensePlate { get; set; } = default!;
    public ICollection<ParkingSession> ParkingSessions { get; set; } = new List<ParkingSession>();
}
