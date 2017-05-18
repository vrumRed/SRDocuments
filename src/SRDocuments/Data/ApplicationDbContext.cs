using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SRDocuments.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SRDocuments.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>()
                .HasAlternateKey(u => u.CPF)
                .HasName("AlternateKey_CPF");

            modelBuilder.Entity<Document>()
                .Property(b => b.Finished)
                .HasDefaultValue(false);

            modelBuilder.Entity<Document>()
                .Property(b => b.NotAccepted)
                .HasDefaultValue(false);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.SentBy)
                .WithMany(s => s.SentDocuments)
                .HasForeignKey(d => d.SentById)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("ForeignKey_Document_SentByUser");

            modelBuilder.Entity<Document>()
                .HasOne(d => d.SentTo)
                .WithMany(s => s.ReceivedDocuments)
                .HasForeignKey(d => d.SentToId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("ForeignKey_Document_SentToUser");

            modelBuilder.Entity<DocumentImage>()
                .HasOne(d => d.Document)
                .WithMany(s => s.DocumentImages)
                .HasForeignKey(d => d.DocumentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("ForeignKey_Document_File");

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.NotificationUser)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.NotificationUserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("ForeignKey_Notification_User");

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .HasComputedColumnSql("[FirstName] + ' ' + [LastName]");

            modelBuilder.Entity<ApplicationUser>()
                .Property(u => u.Info)
                .HasComputedColumnSql("[FirstName] + ' ' + [LastName] + ': ' + [Email]");

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentImage> DocumentImages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
    }
}
