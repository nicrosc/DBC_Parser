using Microsoft.EntityFrameworkCore;
using ZadatakJuric.Domain.Entities;
using ZadatakJuric.Domain.Enums;
using System.Text.Json;

namespace ZadatakJuric.Infrastructure.Data;

public class DBCDbContext : DbContext
{
    public DBCDbContext(DbContextOptions<DBCDbContext> options) : base(options)
    {
    }

    public DbSet<DBC> DBCFiles { get; set; }
    public DbSet<Network> Networks { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Signal> Signals { get; set; }
    public DbSet<Domain.Entities.Attribute> Attributes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure DBC entity
        modelBuilder.Entity<DBC>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Version).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.FileHash).HasMaxLength(64); // SHA256 hash
            
            // Configure one-to-many relationship with Networks
            entity.HasMany(d => d.Networks)
                  .WithOne(n => n.DBC)
                  .HasForeignKey(n => n.DBCId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationship with Attributes
            entity.HasMany(d => d.Attributes)
                  .WithOne(a => a.DBC)
                  .HasForeignKey(a => a.DBCId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Network entity
        modelBuilder.Entity<Network>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DBCId).IsRequired();

            // Configure one-to-many relationship with Messages
            entity.HasMany(n => n.Messages)
                  .WithOne(m => m.Network)
                  .HasForeignKey(m => m.NetworkId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationship with Attributes  
            entity.HasMany(n => n.Attributes)
                  .WithOne(a => a.Network)
                  .HasForeignKey(a => a.NetworkId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Message entity
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.MessageId).IsRequired(); // CAN message ID
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Size).IsRequired();
            entity.Property(e => e.Sender).HasMaxLength(100);
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.NetworkId).IsRequired();

            // Configure one-to-many relationship with Signals
            entity.HasMany(m => m.Signals)
                  .WithOne(s => s.Message)
                  .HasForeignKey(s => s.MessageId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationship with Attributes
            entity.HasMany(m => m.Attributes)
                  .WithOne(a => a.Message)
                  .HasForeignKey(a => a.MessageId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Signal entity
        modelBuilder.Entity<Signal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.StartBit).IsRequired();
            entity.Property(e => e.Length).IsRequired();
            entity.Property(e => e.ByteOrder).HasConversion<string>();
            entity.Property(e => e.ValueType).HasConversion<string>();
            entity.Property(e => e.Factor).IsRequired();
            entity.Property(e => e.Offset).IsRequired();
            entity.Property(e => e.Minimum).IsRequired();
            entity.Property(e => e.Maximum).IsRequired();
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.Comment).HasMaxLength(1000);
            entity.Property(e => e.MessageId).IsRequired();

            // Configure Receivers as comma-separated string with value comparer
            entity.Property(e => e.Receivers)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                  )
                  .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                      (c1, c2) => c1.SequenceEqual(c2),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()));

            // Configure one-to-many relationship with Attributes
            entity.HasMany(s => s.Attributes)
                  .WithOne(a => a.Signal)
                  .HasForeignKey(a => a.SignalId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Attribute entity
        modelBuilder.Entity<Domain.Entities.Attribute>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ValueType).HasConversion<string>();
            entity.Property(e => e.StringValue).HasMaxLength(500);
            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.DefaultValue).HasMaxLength(500);

            // Configure EnumValues as comma-separated string with value comparer
            entity.Property(e => e.EnumValues)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                  )
                  .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                      (c1, c2) => c1.SequenceEqual(c2),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()));

            // Configure optional foreign keys for polymorphic relationships
            entity.Property(e => e.DBCId);
            entity.Property(e => e.NetworkId);
            entity.Property(e => e.MessageId); 
            entity.Property(e => e.SignalId);
        });
    }
}

// Add Id properties to domain entities for EF Core - placed in separate file to avoid namespace conflict 