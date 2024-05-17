using ApplicationRent.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ApplicationRent.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationIdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Place>().Property(z => z.Id).UseIdentityColumn();
            builder.Entity<Place>().Property(z => z.Name).HasMaxLength(100);
            builder.Entity<Place>().Property(p => p.Price).HasColumnType("decimal(18,2)");

            /*builder.Entity<Place>().HasData(
                new Place
                {
                    Id = 1,
                    Name = "First",
                    StartRent = new DateTime(2024, 02, 01),
                    EndRent = new DateTime(2024, 02, 25),
                    InRent = true,
                    Price = 1500.500m,
                    SizePlace = 15.5,
                    Description = "Комната 15 метров",
                    Category = "Склад",
                });*/

            // Конфигурация для свойства Balance в ApplicationIdentityUser
            builder.Entity<ApplicationIdentityUser>()
                   .Property(u => u.Balance)
                   .HasColumnType("decimal(18, 2)");

            // Конфигурация для сущности TransactionHistory
            builder.Entity<TransactionHistory>()
                   .Property(t => t.Amount)
                   .HasColumnType("decimal(18, 2)");

            builder.Entity<TransactionHistory>()
                   .HasOne(t => t.User)
                   .WithMany()
                   .HasForeignKey(t => t.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(builder);
        }

        public DbSet<Place> Places { get; set; }
        public DbSet<Rental> Rentals { get; set; }
        public DbSet<RequestsRent> RequestsRents { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<TransactionHistory> TransactionHistories { get; set; }

    }
}
