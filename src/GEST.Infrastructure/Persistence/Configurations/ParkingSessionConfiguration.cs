using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEST.Infrastructure.Persistence.Configurations;

public class ParkingSessionConfiguration : IEntityTypeConfiguration<ParkingSession>
{
    public void Configure(EntityTypeBuilder<ParkingSession> builder)
    {
        builder.ToTable("ParkingSessions");

        builder.HasKey(x => x.Id)
            .HasName("PK_ParkingSession");

        builder.Property(x => x.EntryTimeUtc)
            .IsRequired();

        builder.Property(x => x.ParkedTimeUtc);

        builder.Property(x => x.ExitTimeUtc);

        builder.Property(x => x.AppliedPricePerHour)
               .HasPrecision(10, 2)
               .IsRequired();

        builder.Property(x => x.TotalAmount)
               .HasPrecision(12, 2);

        builder.Property(x => x.Multiplier)
               .HasPrecision(18, 2);

        builder.Property(x => x.PricingTier)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(x => x.Status)
               .HasConversion<int>()
               .IsRequired();

        builder.HasOne(s => s.Sector)
               .WithMany(x => x.ParkingSessions)
               .HasForeignKey(x => x.SectorId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_ParkingSession_Sector");

        builder.HasOne(s => s.Spot)
               .WithMany(x => x.ParkingSessions)
               .HasForeignKey(x => x.SpotId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_ParkingSession_Spot");

        builder.HasOne(s => s.Vehicle)
               .WithMany(x => x.ParkingSessions)
               .HasForeignKey(x => x.VehicleId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_ParkingSession_Vehicle");

        builder.HasIndex(x => new { x.VehicleId, x.Status })
               .HasDatabaseName("IX_Session_ByVehicle_Status");

        builder.HasIndex(x => new { x.SectorId, x.ExitTimeUtc })
               .HasDatabaseName("IX_Session_BySector_ExitTime");
    }
}