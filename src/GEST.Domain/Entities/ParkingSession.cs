using GEST.Domain.Enums;

namespace GEST.Domain.Entities;

public class ParkingSession
{
    public Guid Id { get; set; }

    // Chaves de negócio
    public string LicensePlate { get; set; } = default!;
    public int? SpotId { get; set; }
    public string SectorCode { get; set; } = default!;

    // Timeline (sempre UTC)
    public DateTime EntryTimeUtc { get; set; }
    public DateTime? ParkedTimeUtc { get; set; }
    public DateTime? ExitTimeUtc { get; set; }

    // Preço decidido na ENTRADA (com tier aplicado naquele instante)
    public decimal AppliedPricePerHour { get; set; }
    public PricingTier PricingTier { get; set; }

    // Valor final calculado no EXIT (mantido para auditoria/consulta de receita)
    public decimal? TotalAmount { get; set; }
    public ParkingStatus Status { get; set; }

    // Navegações
    public Vehicle Vehicle { get; set; } = default!;
    public Spot? Spot { get; set; }
    public Sector Sector { get; set; } = default!;
}
