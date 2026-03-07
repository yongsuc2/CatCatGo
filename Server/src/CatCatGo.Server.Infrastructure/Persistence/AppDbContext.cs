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
    public DbSet<ResourceBalance> ResourceBalances => Set<ResourceBalance>();
    public DbSet<ResourceLedger> ResourceLedgers => Set<ResourceLedger>();
    public DbSet<TalentState> TalentStates => Set<TalentState>();
    public DbSet<EquipmentEntry> EquipmentEntries => Set<EquipmentEntry>();
    public DbSet<ChapterSession> ChapterSessions => Set<ChapterSession>();
    public DbSet<ChapterProgress> ChapterProgresses => Set<ChapterProgress>();
    public DbSet<GachaPity> GachaPities => Set<GachaPity>();
    public DbSet<PetEntry> PetEntries => Set<PetEntry>();
    public DbSet<HeritageState> HeritageStates => Set<HeritageState>();
    public DbSet<DailyAttendance> DailyAttendances => Set<DailyAttendance>();
    public DbSet<QuestProgress> QuestProgresses => Set<QuestProgress>();
    public DbSet<ContentProgress> ContentProgresses => Set<ContentProgress>();

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

        modelBuilder.Entity<ResourceBalance>(entity =>
        {
            entity.ToTable("resource_balances");
            entity.HasKey(e => new { e.AccountId, e.Type });
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId);
        });

        modelBuilder.Entity<ResourceLedger>(entity =>
        {
            entity.ToTable("resource_ledger");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.AccountId, e.CreatedAt });
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId);
        });

        modelBuilder.Entity<TalentState>(entity =>
        {
            entity.ToTable("talent_states");
            entity.HasKey(e => e.AccountId);
            entity.HasOne(e => e.Account).WithOne().HasForeignKey<TalentState>(e => e.AccountId);
            entity.Property(e => e.ClaimedMilestones).HasColumnType("jsonb");
        });

        modelBuilder.Entity<EquipmentEntry>(entity =>
        {
            entity.ToTable("equipment_registry");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AccountId);
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId);
            entity.Property(e => e.SubStats).HasColumnType("jsonb");
        });

        modelBuilder.Entity<ChapterSession>(entity =>
        {
            entity.ToTable("chapter_sessions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.AccountId, e.IsActive });
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId);
            entity.Property(e => e.SessionSkills).HasColumnType("jsonb");
            entity.Property(e => e.PendingEncounter).HasColumnType("jsonb");
            entity.Property(e => e.PendingSkillChoices).HasColumnType("jsonb");
        });

        modelBuilder.Entity<ChapterProgress>(entity =>
        {
            entity.ToTable("chapter_progresses");
            entity.HasKey(e => e.AccountId);
            entity.HasOne(e => e.Account).WithOne().HasForeignKey<ChapterProgress>(e => e.AccountId);
            entity.Property(e => e.BestSurvivalDays).HasColumnType("jsonb");
            entity.Property(e => e.ClaimedTreasures).HasColumnType("jsonb");
        });

        modelBuilder.Entity<GachaPity>(entity =>
        {
            entity.ToTable("gacha_pity_counters");
            entity.HasKey(e => new { e.AccountId, e.BoxType });
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId);
        });

        modelBuilder.Entity<PetEntry>(entity =>
        {
            entity.ToTable("pet_registry");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AccountId);
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId);
        });

        modelBuilder.Entity<HeritageState>(entity =>
        {
            entity.ToTable("heritage_states");
            entity.HasKey(e => e.AccountId);
            entity.HasOne(e => e.Account).WithOne().HasForeignKey<HeritageState>(e => e.AccountId);
        });

        modelBuilder.Entity<DailyAttendance>(entity =>
        {
            entity.ToTable("daily_attendance");
            entity.HasKey(e => e.AccountId);
            entity.HasOne(e => e.Account).WithOne().HasForeignKey<DailyAttendance>(e => e.AccountId);
        });

        modelBuilder.Entity<QuestProgress>(entity =>
        {
            entity.ToTable("daily_quests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.HasIndex(e => new { e.AccountId, e.QuestType, e.ResetDate });
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId);
        });

        modelBuilder.Entity<ContentProgress>(entity =>
        {
            entity.ToTable("content_daily_counts");
            entity.HasKey(e => new { e.AccountId, e.ContentType });
            entity.HasOne(e => e.Account).WithMany().HasForeignKey(e => e.AccountId);
        });
    }
}
