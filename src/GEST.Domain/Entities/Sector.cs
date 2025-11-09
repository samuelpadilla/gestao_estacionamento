namespace GEST.Domain.Entities;

public class Sector
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public decimal BasePrice { get; set; }
    public int MaxCapacity { get; set; }

    public ICollection<Spot> Spots { get; set; } = [];
    public ICollection<ParkingSession> ParkingSessions { get; set; } = [];
}