using GEST.Domain.Enums;

namespace GEST.Domain.Entities;

public class ParkingSession
{
    public Guid Id { get; set; }
    public int? SectorId { get; set; }
    public Sector Sector { get; set; }
    public int? SpotId { get; set; }
    public Spot? Spot { get; set; }
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; }

    public DateTime EntryTimeUtc { get; set; }
    public DateTime? ParkedTimeUtc { get; set; }
    public DateTime? ExitTimeUtc { get; set; }

    public decimal Multiplier { get; set; }
    public PricingTier PricingTier { get; set; }
    public decimal AppliedPricePerHour { get; set; }

    // Valor final calculado no EXIT (mantido para auditoria/consulta de receita)
    public decimal? TotalAmount { get; set; }
    public ParkingStatus Status { get; set; }
}
