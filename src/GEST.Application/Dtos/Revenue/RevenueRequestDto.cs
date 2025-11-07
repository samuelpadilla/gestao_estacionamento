namespace GEST.Application.Dtos.Revenue;

public sealed class RevenueRequestDto
{
    // "2025-01-01"
    public DateOnly Date { get; set; }
    public string Sector { get; set; } = default!;
}