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
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<ApplicationUser>()
                .HasAlternateKey(u => u.CPF)
                .HasName("AlternateKey_CPF");

            modelBuilder.Entity<Document>()
                .Property(b => b.Finished)
                .HasDefaultValue(false);

            modelBuilder.Entity<Document>()
                .Property(b => b.NotAccepted)
                .HasDefaultValue(false);

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
