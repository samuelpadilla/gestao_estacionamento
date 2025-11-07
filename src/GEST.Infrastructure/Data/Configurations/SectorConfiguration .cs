using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEST.Infrastructure.Data.Configurations;

public class SectorConfiguration : IEntityTypeConfiguration<Sector>
{
    public void Configure(EntityTypeBuilder<Sector> builder)
    {
        builder.ToTable("Sectors");

        builder.HasKey(x => x.Code);

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
               .HasForeignKey(s => s.SectorCode)
               .OnDelete(DeleteBehavior.Restrict);
    }
}