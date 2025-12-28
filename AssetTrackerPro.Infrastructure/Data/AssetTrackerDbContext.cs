using AssetTrackerPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerPro.Infrastructure.Data;

public class AssetTrackerDbContext : DbContext
{
    public AssetTrackerDbContext(DbContextOptions<AssetTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Departments> Departments => Set<Departments>();
    public DbSet<Employees> Employees => Set<Employees>();
    public DbSet<Assets> Assets => Set<Assets>();
    public DbSet<Users> Users => Set<Users>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Departments>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(d => d.Id);
            entity.Property(d => d.DeptName).IsRequired().HasMaxLength(200);
            entity.Property(d => d.Location).HasMaxLength(200);
            entity.HasIndex(d => d.DeptName).IsUnique();

            entity.HasMany(d => d.Employees)
                .WithOne(e => e.Department)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Employees>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(320);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasMany(e => e.Assets)
                .WithOne(a => a.Employee)
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Assets>(entity =>
        {
            entity.ToTable("Assets");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AssetName).IsRequired().HasMaxLength(200);
            entity.Property(a => a.SerialNumber).IsRequired().HasMaxLength(100);
            entity.Property(a => a.AssetType).IsRequired().HasMaxLength(100);
            entity.Property(a => a.PurchaseDate).IsRequired();
            entity.HasIndex(a => a.SerialNumber).IsUnique();
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
            entity.HasIndex(u => u.Username).IsUnique();
        });
    }
}
