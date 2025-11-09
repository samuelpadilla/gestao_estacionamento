using System.ComponentModel.DataAnnotations;

namespace GEST.Domain.Entities;

public class Spot
{
    public int Id { get; set; }
    public int SectorId { get; set; }
    public Sector Sector { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }

    // Estado atual (derivado dos eventos/sessões); mantemos para leituras rápidas
    public bool IsOccupied { get; set; }
    public string? CurrentLicensePlate { get; set; }
    [Timestamp]
    public byte[] RowVersion { get; set; } = [];

    public ICollection<ParkingSession> ParkingSessions { get; set; } = [];
}
