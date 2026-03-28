using Enhanzer.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.Api.Data;


public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<LocationDetail> LocationDetails => Set<LocationDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LocationDetail>(entity =>
        {
            entity.ToTable("Location_Details");
            entity.HasKey(location => location.Id);
            entity.Property(location => location.Location_Code)
                .HasMaxLength(100)
                .IsRequired();
            entity.Property(location => location.Location_Name)
                .HasMaxLength(200)
                .IsRequired();
            entity.Property(location => location.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(location => location.UpdatedAt);
        });
    }
}