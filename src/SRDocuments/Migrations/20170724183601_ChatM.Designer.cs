using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SRDocuments.Data;

namespace SRDocuments.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20170724183601_ChatM")]
    partial class ChatM
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("SRDocuments.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id");

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("BlockToken");

                    b.Property<string>("CPF")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 11);

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 80);

                    b.Property<string>("FullName")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasComputedColumnSql("[FirstName] + ' ' + [LastName]")
                        .HasAnnotation("MaxLength", 160);

                    b.Property<string>("Info")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasComputedColumnSql("[FirstName] + ' ' + [LastName] + ': ' + [Email]")
                        .HasAnnotation("MaxLength", 1200);

                    b.Property<bool>("IsBlocked");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 80);

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedUserName")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UnblockToken");

                    b.Property<string>("UserName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasAlternateKey("CPF")
                        .HasName("AlternateKey_CPF");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("SRDocuments.Models.Chat", b =>
                {
                    b.Property<int>("ChatID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DocumentID");

                    b.Property<string>("Person1ID")
                        .IsRequired();

                    b.Property<string>("Person2ID")
                        .IsRequired();

                    b.HasKey("ChatID");

                    b.HasIndex("DocumentID");

                    b.HasIndex("Person1ID");

                    b.HasIndex("Person2ID");

                    b.ToTable("Chats");
                });

            modelBuilder.Entity("SRDocuments.Models.Document", b =>
                {
                    b.Property<int>("DocumentID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AnswerDate")
                        .HasAnnotation("MaxLength", 15);

                    b.Property<string>("ConclusionDate")
                        .HasAnnotation("MaxLength", 15);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 999);

                    b.Property<bool>("Finished")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 160);

                    b.Property<bool>("NotAccepted")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(false);

                    b.Property<string>("ReceivedImagesRarLocale");

                    b.Property<string>("RequiredDate")
                        .HasAnnotation("MaxLength", 15);

                    b.Property<string>("SentByID")
                        .IsRequired();

                    b.Property<string>("SentDate")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 15);

                    b.Property<string>("SentImagesRarLocale");

                    b.Property<string>("SentToID")
                        .IsRequired();

                    b.Property<string>("VisualizationDate")
                        .HasAnnotation("MaxLength", 15);

                    b.HasKey("DocumentID");

                    b.HasIndex("SentByID");

                    b.HasIndex("SentToID");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("SRDocuments.Models.DocumentImage", b =>
                {
                    b.Property<int>("DocumentImageID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DateSent")
                        .IsRequired();

                    b.Property<int>("DocumentID");

                    b.Property<string>("Locale")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 160);

                    b.Property<bool>("Original");

                    b.HasKey("DocumentImageID");

                    b.HasIndex("DocumentID");

                    b.ToTable("DocumentImages");
                });

            modelBuilder.Entity("SRDocuments.Models.Message", b =>
                {
                    b.Property<int>("MessageID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ChatID");

                    b.Property<string>("SentByID")
                        .IsRequired();

                    b.Property<string>("SentDate")
                        .IsRequired();

                    b.Property<string>("Text")
                        .IsRequired();

                    b.Property<string>("VisualizationDate");

                    b.HasKey("MessageID");

                    b.HasIndex("ChatID");

                    b.HasIndex("SentByID");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("SRDocuments.Models.Notification", b =>
                {
                    b.Property<int>("NotificationID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Message")
                        .IsRequired();

                    b.Property<string>("NotificationUserID")
                        .IsRequired();

                    b.Property<bool>("wasRead");

                    b.HasKey("NotificationID");

                    b.HasIndex("NotificationUserID");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("SRDocuments.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("SRDocuments.Models.ApplicationUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId");

                    b.HasOne("SRDocuments.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("SRDocuments.Models.Chat", b =>
                {
                    b.HasOne("SRDocuments.Models.Document", "Document")
                        .WithMany()
                        .HasForeignKey("DocumentID");

                    b.HasOne("SRDocuments.Models.ApplicationUser", "Person1")
                        .WithMany()
                        .HasForeignKey("Person1ID");

                    b.HasOne("SRDocuments.Models.ApplicationUser", "Person2")
                        .WithMany()
                        .HasForeignKey("Person2ID");
                });

            modelBuilder.Entity("SRDocuments.Models.Document", b =>
                {
                    b.HasOne("SRDocuments.Models.ApplicationUser", "SentBy")
                        .WithMany()
                        .HasForeignKey("SentByID");

                    b.HasOne("SRDocuments.Models.ApplicationUser", "SentTo")
                        .WithMany()
                        .HasForeignKey("SentToID");
                });

            modelBuilder.Entity("SRDocuments.Models.DocumentImage", b =>
                {
                    b.HasOne("SRDocuments.Models.Document", "Document")
                        .WithMany()
                        .HasForeignKey("DocumentID");
                });

            modelBuilder.Entity("SRDocuments.Models.Message", b =>
                {
                    b.HasOne("SRDocuments.Models.Chat", "Chat")
                        .WithMany()
                        .HasForeignKey("ChatID");

                    b.HasOne("SRDocuments.Models.ApplicationUser", "SentBy")
                        .WithMany()
                        .HasForeignKey("SentByID");
                });

            modelBuilder.Entity("SRDocuments.Models.Notification", b =>
                {
                    b.HasOne("SRDocuments.Models.ApplicationUser", "NotificationUser")
                        .WithMany()
                        .HasForeignKey("NotificationUserID");
                });
        }
    }
}
