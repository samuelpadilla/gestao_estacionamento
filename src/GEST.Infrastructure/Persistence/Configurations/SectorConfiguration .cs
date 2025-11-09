using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEST.Infrastructure.Persistence.Configurations;

public class SectorConfiguration : IEntityTypeConfiguration<Sector>
{
    public void Configure(EntityTypeBuilder<Sector> builder)
    {
        builder.ToTable("Sectors");

        builder.HasKey(x => x.Id)
            .HasName("PK_Sector");

        builder.Property(x => x.Code)
               .HasMaxLength(10)
               .IsRequired();

        builder.Property(x => x.BasePrice)
               .HasPrecision(10, 2)
               .IsRequired();

        builder.Property(x => x.MaxCapacity)
               .IsRequired();

        builder.HasMany(x => x.Spots)
               .WithOne(s => s.Sector)
               .HasForeignKey(s => s.SectorId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Sector_Spot");

        builder.HasMany(x => x.ParkingSessions)
               .WithOne(s => s.Sector)
               .HasForeignKey(s => s.SectorId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Sector_ParkingSession");
    }
}