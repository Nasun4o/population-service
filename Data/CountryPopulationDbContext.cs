using Microsoft.EntityFrameworkCore;
using PopulationService.Data.Entities;

namespace PopulationService.Data;

public class CountryPopulationDbContext : DbContext
{
    public CountryPopulationDbContext(DbContextOptions<CountryPopulationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CountryEntity> Countries => Set<CountryEntity>();
    public DbSet<StateEntity> States => Set<StateEntity>();
    public DbSet<CityEntity> Cities => Set<CityEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CountryEntity>(entity =>
        {
            entity.ToTable("Country");
            entity.HasKey(e => e.CountryId);
            entity.Property(e => e.CountryName)
            .HasMaxLength(2000);
        });

        modelBuilder.Entity<StateEntity>(entity =>
        {
            entity.ToTable("State");
            entity.HasKey(e => e.StateId);
            entity.Property(e => e.StateName)
            .HasMaxLength(2000)
            .IsRequired();

            entity.HasOne(e => e.Country)
                  .WithMany(c => c.States)
                  .HasForeignKey(e => e.CountryId);
        });

        modelBuilder.Entity<CityEntity>(entity =>
        {
            entity.ToTable("City");
            entity.HasKey(e => e.CityId);
            entity.Property(e => e.CityName)
            .HasMaxLength(2000)
            .IsRequired();

            entity.HasOne(e => e.State)
                  .WithMany(s => s.Cities)
                  .HasForeignKey(e => e.StateId);
        });
    }
}
