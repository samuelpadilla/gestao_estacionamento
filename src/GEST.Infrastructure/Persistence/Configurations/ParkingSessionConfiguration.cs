using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEST.Infrastructure.Persistence.Configurations;

public class ParkingSessionConfiguration : IEntityTypeConfiguration<ParkingSession>
{
    public void Configure(EntityTypeBuilder<ParkingSession> builder)
    {
        builder.ToTable("ParkingSessions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LicensePlate)
               .HasMaxLength(12)
               .IsRequired();

        builder.Property(x => x.SectorCode)
               .HasMaxLength(10)
               .IsRequired();

        builder.Property(x => x.EntryTimeUtc).IsRequired();
        builder.Property(x => x.ParkedTimeUtc);
        builder.Property(x => x.ExitTimeUtc);

        builder.Property(x => x.AppliedPricePerHour)
               .HasPrecision(10, 2)
               .IsRequired();

        builder.Property(x => x.TotalAmount)
               .HasPrecision(12, 2);

        builder.Property(x => x.PricingTier)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(x => x.Status)
               .HasConversion<int>()
               .IsRequired();

        builder.HasOne(x => x.Sector)
               .WithMany()
               .HasForeignKey(x => x.SectorCode)
               .OnDelete(DeleteBehavior.Restrict);

        // Uma sessão ativa por placa (ExitTime NULL)
        builder.HasIndex(x => new { x.LicensePlate, x.Status })
               .HasDatabaseName("IX_Session_ActiveByPlate");

        // Consultas por data/sector para /revenue
        builder.HasIndex(x => new { x.SectorCode, x.ExitTimeUtc });
    }
}