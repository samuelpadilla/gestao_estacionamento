using System.ComponentModel.DataAnnotations;

namespace GEST.Domain.Entities;

public class Spot
{
    public int Id { get; set; }
    public string SectorCode { get; set; } = default!;
    public double Lat { get; set; }
    public double Lng { get; set; }

    // Estado atual (derivado dos eventos/sessões); mantemos para leituras rápidas
    public bool IsOccupied { get; set; }
    public string? CurrentLicensePlate { get; set; }

    // Concurrency token para evitar colisões de atualização
    [Timestamp]
    public byte[] RowVersion { get; set; } = [];

    public Sector Sector { get; set; } = default!;
    public ICollection<ParkingSession> ParkingSessions { get; set; } = [];
}
