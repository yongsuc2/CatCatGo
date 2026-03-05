using CatCatGo.Server.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CatCatGo.Server.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<ServerSaveData> SaveData => Set<ServerSaveData>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<ArenaRanking> ArenaRankings => Set<ArenaRanking>();
    public DbSet<CheatFlag> CheatFlags => Set<CheatFlag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("accounts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DeviceId).IsUnique();
            entity.HasIndex(e => new { e.SocialType, e.SocialId }).IsUnique().HasFilter("social_type IS NOT NULL");
        });

        modelBuilder.Entity<ServerSaveData>(entity =>
        {
            entity.ToTable("save_data");
            entity.HasKey(e => e.AccountId);
            entity.HasOne(e => e.Account).WithOne().HasForeignKey<ServerSaveData>(e => e.AccountId);
            entity.Property(e => e.Data).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Rewards).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Purchase>(entity =>
        {
            entity.ToTable("purchases");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ReceiptId).IsUnique();
            entity.HasIndex(e => e.AccountId);
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId);
            entity.HasOne(e => e.Product).WithMany().HasForeignKey(e => e.ProductId);
        });

        modelBuilder.Entity<ArenaRanking>(entity =>
        {
            entity.ToTable("arena_rankings");
            entity.HasKey(e => e.AccountId);
            entity.HasIndex(e => new { e.Tier, e.Points });
            entity.HasIndex(e => new { e.Season, e.Points });
            entity.HasOne(e => e.Account).WithOne().HasForeignKey<ArenaRanking>(e => e.AccountId);
            entity.Property(e => e.PlayerData).HasColumnType("jsonb");
        });

        modelBuilder.Entity<CheatFlag>(entity =>
        {
            entity.ToTable("cheat_flags");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AccountId);
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId);
            entity.Property(e => e.Details).HasColumnType("jsonb");
        });
    }
}
