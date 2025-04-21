using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.ApplicationServices;

namespace Garbage_Collector.Model;

public partial class GarbageCollectorDbContext : DbContext
{
    public GarbageCollectorDbContext()
    {
    }

    public GarbageCollectorDbContext(DbContextOptions<GarbageCollectorDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CleanupLog> CleanupLogs { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = AppConfig.LoadFromJson();
            optionsBuilder.UseSqlServer(config.ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CleanupLog>(entity =>
        {
            entity.HasKey(e => e.CleanupLogId).HasName("PK__CleanupL__33B67D89C437F914");

            entity.Property(e => e.CleanupDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CleanupType)
                .HasMaxLength(50)
                .HasDefaultValue("Standard");
            entity.Property(e => e.SpaceFreedInMb).HasColumnName("SpaceFreedInMB");

            entity.HasOne(d => d.User).WithMany(p => p.CleanupLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__CleanupLo__UserI__7BE56230");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1AB4E0B91A");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160FAC7BD11").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C1FCD3355");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__UserRole__3D978A3517636FDC");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__UserRoles__RoleI__7908F585");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserRoles__UserI__7814D14C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
