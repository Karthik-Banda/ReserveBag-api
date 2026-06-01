using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReserveBag.Models
{
    // This strictly defines exactly what we expect from the React Frontend
    public class ReservationCreateDto
    {
        [Required]
        [StringLength (100)]
        public string? CustomerName { get; set; }

        [Required]
        [Phone]
        public string? CustomerPhone { get; set; }

        [Required]
        public List<ReservationItemDto>? Items { get; set; }
    }

    public class ReservationItemDto
    {
        [Required]
        public int VariantId { get; set; }

        [Required]
        [Range (1, 50)]
        public int Quantity { get; set; }
    }

    public class ProductCreateDto
    {
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public string? ImageUrl { get; set; }
        [Required]
        public int StockQuantity { get; set; }
    }

    public class RestockDto
    {
        [Required]
        public int VariantId { get; set; }
        [Required]
        public int AddQuantity { get; set; }
    }
}