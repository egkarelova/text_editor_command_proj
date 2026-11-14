using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TextEditor.Data.Entities;

namespace TextEditor.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<UserSettingsEntity> UserSettings => Set<UserSettingsEntity>();
    public DbSet<DocumentRegistryEntity> Documents => Set<DocumentRegistryEntity>();
    public DbSet<DocumentContentEntity> DocumentContents => Set<DocumentContentEntity>();
    public DbSet<DocumentVersionEntity> DocumentVersions => Set<DocumentVersionEntity>();
    public DbSet<AutoReplacementEntity> AutoReplacements => Set<AutoReplacementEntity>();
    public DbSet<ForbiddenWordEntity> ForbiddenWords => Set<ForbiddenWordEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(e =>
        {
            e.ToTable("Users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Login).IsRequired().HasMaxLength(100);
            e.Property(x => x.IsGuest).HasDefaultValue(false);
            e.HasIndex(x => x.Login).IsUnique();
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<UserSettingsEntity>(e =>
        {
            e.ToTable("UserSettings");
            e.HasKey(x => x.UserId);
            e.HasOne(x => x.User)
             .WithOne(x => x.Settings)
             .HasForeignKey<UserSettingsEntity>(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Theme).IsRequired().HasDefaultValue("Light");
            e.Property(x => x.DefaultFont).IsRequired().HasDefaultValue("Segoe UI");
            e.Property(x => x.DefaultFontSize).HasDefaultValue(14);
            e.Property(x => x.AutoSaveEnabled).HasDefaultValue(true);
            e.Property(x => x.AutoSaveMinutes).HasDefaultValue(5);
        });

        modelBuilder.Entity<DocumentRegistryEntity>(e =>
        {
            e.ToTable("DocumentRegistry");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.User)
             .WithMany(x => x.Documents)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Format).IsRequired().HasMaxLength(3);
            e.Property(x => x.IsShared).HasDefaultValue(false);
            e.HasIndex(x => new { x.UserId, x.UpdatedAt });
            e.HasIndex(x => x.IsShared);
        });

        modelBuilder.Entity<DocumentContentEntity>(e =>
        {
            e.ToTable("DocumentContents");
            e.HasKey(x => x.DocumentId);
            e.HasOne(x => x.Document)
             .WithOne(x => x.Content)
             .HasForeignKey<DocumentContentEntity>(x => x.DocumentId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.Content).IsRequired();
            e.ToTable(t => t.HasCheckConstraint("CK_DocumentContents_ByteSize", "ByteSize <= 1048576"));
        });

        modelBuilder.Entity<DocumentVersionEntity>(e =>
        {
            e.ToTable("DocumentVersions");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Document)
             .WithMany(x => x.Versions)
             .HasForeignKey(x => x.DocumentId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.DocumentId, x.VersionNumber }).IsUnique();
            e.Property(x => x.Content).IsRequired();
        });

        modelBuilder.Entity<AutoReplacementEntity>(e =>
        {
            e.ToTable("AutoReplacements");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.Shortcut }).IsUnique();
            e.Property(x => x.Shortcut).IsRequired().HasMaxLength(50);
            e.Property(x => x.Expansion).IsRequired();
        });

        modelBuilder.Entity<ForbiddenWordEntity>(e =>
        {
            e.ToTable("ForbiddenWords");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Word).IsUnique();
            e.Property(x => x.Word).IsRequired().HasMaxLength(200);
        });
    }
}