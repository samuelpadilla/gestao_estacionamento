using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEST.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(x => x.Id)
            .HasName("PK_Vehicle");

        builder.Property(x => x.LicensePlate)
               .HasMaxLength(12)
               .IsRequired();

        builder.HasMany(x => x.ParkingSessions)
               .WithOne(s => s.Vehicle)
               .HasForeignKey(s => s.VehicleId)
               .OnDelete(DeleteBehavior.Restrict)
               .HasConstraintName("FK_Vehicle_ParkingSession");

        builder.HasIndex(x => x.LicensePlate)
            .IsUnique()
            .HasDatabaseName("IX_Vehicle_ByLicensePlate");
    }
}
