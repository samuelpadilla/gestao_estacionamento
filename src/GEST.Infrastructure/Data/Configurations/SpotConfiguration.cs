using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEST.Infrastructure.Data.Configurations;

public class SpotConfiguration : IEntityTypeConfiguration<Spot>
{
    public void Configure(EntityTypeBuilder<Spot> builder)
    {
        builder.ToTable("Spots");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SectorCode)
               .HasMaxLength(10)
               .IsRequired();

        builder.Property(x => x.Lat).IsRequired();
        builder.Property(x => x.Lng).IsRequired();

        builder.Property(x => x.IsOccupied).IsRequired();

        builder.Property(x => x.CurrentLicensePlate)
               .HasMaxLength(12);

        builder.Property(x => x.RowVersion)
               .IsRowVersion();

        builder.HasIndex(x => new { x.SectorCode, x.IsOccupied });

        // Otimiza “vagas por setor”
        builder.HasIndex(x => x.SectorCode);

        // Navegação
        builder.HasMany(x => x.ParkingSessions)
               .WithOne(s => s.Spot)
               .HasForeignKey(s => s.SpotId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
