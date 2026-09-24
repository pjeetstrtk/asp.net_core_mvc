using BillingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BillingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Billing> Billings => Set<Billing>();
        public DbSet<BillingItem> BillingItems => Set<BillingItem>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Explicit table names (works for MySQL, PostgreSQL, SQL Server)
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Billing>().ToTable("Billings");
            modelBuilder.Entity<BillingItem>().ToTable("BillingItems");
            modelBuilder.Entity<Payment>().ToTable("Payments");

            // Billing -> Customer (Restrict delete to protect history)
            modelBuilder.Entity<Billing>()
                .HasOne(b => b.Customer)
                .WithMany(c => c.Billings)
                .HasForeignKey(b => b.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // BillingItem -> Billing (Cascade delete)
            modelBuilder.Entity<BillingItem>()
                .HasOne(bi => bi.Billing)
                .WithMany(b => b.Items)
                .HasForeignKey(bi => bi.BillingId)
                .OnDelete(DeleteBehavior.Cascade);

            // BillingItem -> Product (Restrict delete to protect history)
            modelBuilder.Entity<BillingItem>()
                .HasOne(bi => bi.Product)
                .WithMany()
                .HasForeignKey(bi => bi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment -> Billing (Cascade delete)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Billing)
                .WithMany(b => b.Payments)
                .HasForeignKey(p => p.BillingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal precision (needed for PostgreSQL; safe for MySQL too)
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Billing>()
                .Property(b => b.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Billing>()
                .Property(b => b.PaidAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Billing>()
                .Property(b => b.BalanceAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BillingItem>()
                .Property(bi => bi.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<BillingItem>()
                .Property(bi => bi.LineTotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);
        }
    }
}