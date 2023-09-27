using Microsoft.EntityFrameworkCore;
using SmartCharging.Domain.Entities;

namespace SmartCharging.Persistence.Context;

public class SmartChargingDbContext : DbContext
{
    public DbSet<Group> Groups { get; set; }
    public DbSet<ChargeStation> ChargeStations { get; set; }
    public DbSet<Connector> Connectors { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // TODO: Put the credentials into config
        optionsBuilder.UseMySQL(
            "server=localhost;port=3306;database=SmartCharging;user=root;password=admin123456");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name).IsRequired();
            entity.Property(g => g.CapacityInAmps).IsRequired();
        });
        
        modelBuilder.Entity<ChargeStation>(entity =>
        {
            entity.HasKey(cs => cs.Id);
            entity.Property(cs => cs.Name).IsRequired();
            entity.HasOne(cs => cs.Group).WithMany(g => g.ChargeStations);
        });
        
        modelBuilder.Entity<Connector>(entity =>
        {
            entity.HasKey("Id","ChargeStationId");
            entity.Property(cs => cs.MaxCurrentInAmps).IsRequired();
            entity.HasOne(cs => cs.ChargeStation).WithMany(g => g.Connectors);
        });
    }
}