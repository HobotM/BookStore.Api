using BookStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Api.Data;

public sealed class BookStoreDbContext : DbContext
{
    public BookStoreDbContext(DbContextOptions<BookStoreDbContext> options) : base(options)
    {
        
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookAudit> BookAudits => Set<BookAudit>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
     protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("Books");

            entity.HasKey(book => book.Id);

            entity.Property(book => book.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(book => book.Author)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(book => book.Price)
                .HasColumnType("decimal(18,2)");

        });

        modelBuilder.Entity<BookAudit>(entity =>
            {
                entity.ToTable("BookAudits");

                entity.HasKey(audit => audit.Id);

                entity.Property(audit => audit.BookId)
                    .IsRequired();

                entity.Property(audit => audit.EventType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(audit => audit.CreatedAtUtc)
                    .IsRequired();

                entity.Property(audit => audit.MessageId)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(audit => audit.MessageId)
                    .IsUnique();
            });

            modelBuilder.Entity<OutboxMessage>(entity =>
            {
                entity.ToTable("OutboxMessages");

                entity.HasKey(message => message.Id);

                entity.Property(message => message.Type)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(message => message.Content)
                    .IsRequired();

                entity.Property(message => message.OccurredAtUtc)
                    .IsRequired();

                entity.Property(message => message.ProcessedAtUtc);

                entity.Property(message => message.Error)
                    .HasMaxLength(2000);

                entity.HasIndex(message => message.ProcessedAtUtc);
            });
    }
    



}

