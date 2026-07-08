using CrimeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
namespace CrimeManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Officer> Officers { get; set; }
        public DbSet<Incident> Incidents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Incident>() .Property(i => i.EstimatedValue) .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Incident>().HasOne(i => i.ReportedBy).WithMany(u => u.Incidents).HasForeignKey(i => i.ReportedByUserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Incident>().HasOne(i => i.AssignedOfficer).WithMany(o => o.AssignedIncidents).HasForeignKey(i => i.AssignedOfficerId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Officer>().HasIndex(o => o.Email).IsUnique();
            modelBuilder.Entity<Officer>().HasIndex(o => o.BadgeNumber).IsUnique();
        }
    }
}