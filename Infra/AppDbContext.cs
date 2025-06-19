using Domain.Entities;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) { }
        public virtual DbSet<ShortUrl> ShortUrls { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var dateTimeConverter = new DateTimeUtcConverter();
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                    }
                }
            }

            // Configure ShortUrl entity
            modelBuilder.Entity<ShortUrl>(entity =>
            {
                entity.ToTable("ShortUrls");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.OriginalUrl)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.Property(e => e.ShortenedUrl)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Team)
                    .IsRequired();

                entity.Property(e => e.Level)
                    .IsRequired();

                entity.Property(e => e.CreateDate)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdateDate)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.IsDeleted)
                    .IsRequired()
                    .HasDefaultValue(false);
                // Create unique index on ShortenedUrl
                entity.HasIndex(e => e.ShortenedUrl)
                    .IsUnique()
                    .HasDatabaseName("IX_ShortUrls_ShortenedUrl");
            });
        }
        
    }
}
