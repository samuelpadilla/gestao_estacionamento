namespace GEST.Application.Dtos.Garage;

public sealed class GarageSectorDto
{
    public string Sector { get; set; } = default!;
    public decimal BasePrice { get; set; }
    public int Max_Capacity { get; set; }
}
