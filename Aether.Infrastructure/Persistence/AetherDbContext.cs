using Aether.Domain.Entities;
using Aether.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aether.Infrastructure.Persistence;

public class AetherDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
{
    public DbSet<Portfolio> Portfolios { get; set; } = null!;
    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<SteamSkinAsset> SteamSkinAssets { get; set; } = null!;
    public DbSet<CryptoAsset> CryptoAssets { get; set; } = null!;

    public AetherDbContext(DbContextOptions<AetherDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Portfolio>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasMany(e => e.Assets)
                  .WithOne()
                  .HasForeignKey("PortfolioId")
                  .OnDelete(DeleteBehavior.Cascade);
            
            var navigation = entity.Metadata.FindNavigation(nameof(Portfolio.Assets));
            navigation?.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

            entity.OwnsOne(e => e.AcquisitionPrice, price =>
            {
                price.Property(p => p.Amount).HasColumnName("AcquisitionAmount").HasPrecision(18, 2);
                price.Property(p => p.Currency).HasColumnName("AcquisitionCurrency").HasMaxLength(10);
            });

            entity.OwnsOne(e => e.CurrentFloorPrice, price =>
            {
                price.Property(p => p.Amount).HasColumnName("CurrentFloorAmount").HasPrecision(18, 2);
                price.Property(p => p.Currency).HasColumnName("CurrentFloorCurrency").HasMaxLength(10);
            });

            entity.HasDiscriminator<string>("AssetType")
                .HasValue<SteamSkinAsset>("SteamSkin")
                .HasValue<CryptoAsset>("Crypto");
        });

        modelBuilder.Entity<SteamSkinAsset>(entity =>
        {
            entity.Property(e => e.MarketHashName).HasMaxLength(500);
            entity.Property(e => e.AppId).HasMaxLength(50);
        });

        modelBuilder.Entity<CryptoAsset>(entity =>
        {
            entity.Property(e => e.Symbol).HasMaxLength(50);
            entity.Property(e => e.Quantity).HasPrecision(30, 18);
        });
    }
}
