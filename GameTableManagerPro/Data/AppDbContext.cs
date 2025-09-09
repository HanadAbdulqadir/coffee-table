using GameTableManagerPro.Models;
using Microsoft.EntityFrameworkCore;

namespace GameTableManagerPro.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<GamingTable> GamingTables { get; set; }
    public DbSet<DeploymentHistory> DeploymentHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<GamingTable>()
            .HasMany(g => g.DeploymentHistory)
            .WithOne(d => d.GamingTable)
            .HasForeignKey(d => d.GamingTableId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure indices for better performance
        modelBuilder.Entity<GamingTable>()
            .HasIndex(g => g.Hostname)
            .IsUnique();

        modelBuilder.Entity<GamingTable>()
            .HasIndex(g => g.IPAddress)
            .IsUnique();

        modelBuilder.Entity<DeploymentHistory>()
            .HasIndex(d => d.GamingTableId);

        modelBuilder.Entity<DeploymentHistory>()
            .HasIndex(d => d.StartTime);
    }
}
