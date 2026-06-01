using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ReserveBag.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? GenderTarget { get; set; }

        [JsonIgnore]
        public ICollection<Product>? Products { get; set; }
    }

    public class Product
    {
        [Key]
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string? Name { get; set; }
        public bool IsArchived { get; set; } = false;
        public string? Description { get; set; }
        public decimal Price { get; set; }

        public Category? Category { get; set; }
        public ICollection<ProductVariant>? Variants { get; set; }
        public ICollection<ProductImage>? Images { get; set; }
    }

    public class ProductVariant
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public int StockQuantity { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; }
    }

    public class ProductImage
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPrimary { get; set; }

        [JsonIgnore]
        public Product? Product { get; set; }
    }

    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        // --- NEW: Added Data Validation Rules ---
        [Required (ErrorMessage = "Customer name is required.")]
        [StringLength (100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string? CustomerName { get; set; }

        [Required (ErrorMessage = "Phone number is required.")]
        [Phone (ErrorMessage = "Invalid phone number format.")]
        [StringLength (20)]
        public string? CustomerPhone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays (2);
        public string Status { get; set; } = "Pending";

        public ICollection<ReservationItem>? Items { get; set; }
    }

    public class ReservationItem
    {
        [Key]
        public int Id { get; set; }
        public int ReservationId { get; set; }
        public int VariantId { get; set; }

        [Range (1, 50, ErrorMessage = "Quantity must be between 1 and 50.")]
        public int Quantity { get; set; }

        [JsonIgnore]
        public Reservation? Reservation { get; set; }

        [JsonIgnore]
        public ProductVariant? Variant { get; set; }
    }
}