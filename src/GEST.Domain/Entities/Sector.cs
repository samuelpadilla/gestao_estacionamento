namespace GEST.Domain.Entities;

public class Sector
{
    public string Code { get; set; } = default!;
    public decimal BasePrice { get; set; }
    public int MaxCapacity { get; set; }

    public ICollection<Spot> Spots { get; set; } = [];
}