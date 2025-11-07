using GEST.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GEST.Infrastructure.Persistence;

public partial class GestDbContext : DbContext
{
    public GestDbContext(DbContextOptions<GestDbContext> options)
        : base(options)
    {
    }
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<Spot> Spots => Set<Spot>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ParkingSession> ParkingSessions => Set<ParkingSession>();
    public DbSet<WebhookEventLog> WebhookEventLogs => Set<WebhookEventLog>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AS");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GestDbContext).Assembly);

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
