using Microsoft.EntityFrameworkCore;
using ReserveBag.Models;

namespace ReserveBag.Data
{
    public class StoreDbContext : DbContext
    {
        public StoreDbContext(DbContextOptions<StoreDbContext> options) : base (options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<ReservationItem> ReservationItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating (modelBuilder);

            modelBuilder.Entity<Product> ()
                .Property (p => p.Price)
                .HasColumnType ("decimal(18,2)");

            // --- DATA SEEDING ---
            // 1. Seed Categories
            modelBuilder.Entity<Category> ().HasData (
                new Category { Id = 1, Name = "Traditional Wear", GenderTarget = "Women" },
                new Category { Id = 2, Name = "Daily Wear", GenderTarget = "Men" },
                new Category { Id = 3, Name = "Festival Wear", GenderTarget = "Kids" }
            );

            // 2. Seed Products
            modelBuilder.Entity<Product> ().HasData (
                new Product { Id = 1, CategoryId = 1, Name = "Silk Embroidered Saree", Description = "Premium silk saree.", Price = 120.00m },
                new Product { Id = 2, CategoryId = 2, Name = "Classic Linen Kurta", Description = "Comfortable daily kurta.", Price = 45.00m },
                new Product { Id = 3, CategoryId = 3, Name = "Kids Festive Lehenga", Description = "Bright festive wear.", Price = 65.00m }
            );

            // 3. Seed Variants (This holds the Stock Quantity!)
            modelBuilder.Entity<ProductVariant> ().HasData (
                new ProductVariant { Id = 1, ProductId = 1, Size = "Free Size", Color = "Red", StockQuantity = 15 },
                new ProductVariant { Id = 2, ProductId = 2, Size = "L", Color = "White", StockQuantity = 3 },
                new ProductVariant { Id = 3, ProductId = 3, Size = "S", Color = "Yellow", StockQuantity = 0 } // Out of stock example
            );

            // 4. Seed Images
            modelBuilder.Entity<ProductImage> ().HasData (
                new ProductImage { Id = 1, ProductId = 1, ImageUrl = "https://images.unsplash.com/photo-1610189014164-9689e472626e?w=500&q=80", IsPrimary = true },
                new ProductImage { Id = 2, ProductId = 2, ImageUrl = "https://images.unsplash.com/photo-1593032465175-481ac7f401a0?w=500&q=80", IsPrimary = true },
                new ProductImage { Id = 3, ProductId = 3, ImageUrl = "https://images.unsplash.com/photo-1622290291468-a28f7a7dc6a8?w=500&q=80", IsPrimary = true }
            );
        }
    }
}