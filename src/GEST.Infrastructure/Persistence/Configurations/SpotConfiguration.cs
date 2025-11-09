using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEST.Infrastructure.Persistence.Configurations;

public class SpotConfiguration : IEntityTypeConfiguration<Spot>
{
    public void Configure(EntityTypeBuilder<Spot> builder)
    {
        builder.ToTable("Spots");

        builder.HasKey(x => x.Id)
            .HasName("PK_Spot");

        builder
            .Property(x => x.Lat)
            .IsRequired();

        builder
            .Property(x => x.Lng)
            .IsRequired();

        builder
            .Property(x => x.IsOccupied)
            .IsRequired();

        builder
            .Property(x => x.CurrentLicensePlate)
            .HasMaxLength(12);

        builder
            .Property(x => x.RowVersion)
            .IsRowVersion();

        builder
            .HasIndex(x => new { x.SectorId, x.IsOccupied })
            .HasDatabaseName("IX_Spot_BySector_IsOccupied");

        builder
            .HasIndex(x => x.SectorId)
            .HasDatabaseName("IX_Spot_BySector");

        builder
            .HasMany(x => x.ParkingSessions)
            .WithOne(s => s.Spot)
            .HasForeignKey(s => s.SpotId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_Spot_ParkingSession");
    }
}
