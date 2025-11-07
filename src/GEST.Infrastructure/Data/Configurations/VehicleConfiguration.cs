using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEST.Infrastructure.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasKey(x => x.LicensePlate);

        builder.Property(x => x.LicensePlate)
               .HasMaxLength(12)
               .IsRequired();

        builder.HasMany(x => x.ParkingSessions)
               .WithOne(s => s.Vehicle)
               .HasForeignKey(s => s.LicensePlate)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.LicensePlate).IsUnique();
    }
}
