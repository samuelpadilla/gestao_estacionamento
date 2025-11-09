namespace GEST.Domain.Entities;

public class Vehicle
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = default!;
    public ICollection<ParkingSession> ParkingSessions { get; set; } = [];
}
