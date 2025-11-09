using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GEST.Infrastructure.Persistence.Configurations;

public class WebhookEventLogConfiguration : IEntityTypeConfiguration<WebhookEventLog>
{
    public void Configure(EntityTypeBuilder<WebhookEventLog> builder)
    {
        builder.ToTable("WebhookEventLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(x => x.LicensePlate)
               .HasMaxLength(12)
               .IsRequired();

        builder.Property(x => x.ReceivedAtUtc)
               .IsRequired();

        builder.Property(x => x.RawPayloadJson)
               .HasColumnType("nvarchar(max)")
               .IsRequired();

        builder
            .HasIndex(x => new { x.LicensePlate, x.ReceivedAtUtc })
            .HasDatabaseName("IX_WebhookEventLog_ByLicensePlate_ReceivedAt");
    }
}
