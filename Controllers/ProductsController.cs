using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReserveBag.Data;
using ReserveBag.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReserveBag.Controllers
{
    [Route ("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly StoreDbContext _context;

        public ProductsController(StoreDbContext context)
        {
            _context = context;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products
                .Include (p => p.Images)
                .Include (p => p.Variants)
                .Include (p => p.Category)
                .AsSplitQuery ()
                .ToListAsync ();

            return Ok (products);
        }

        // GET: api/products/5
        [HttpGet ("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include (p => p.Images)
                .Include (p => p.Variants)
                .Include (p => p.Category)
                .FirstOrDefaultAsync (p => p.Id == id);

            if ( product == null ) return NotFound ();

            return Ok (product);
        }

        // --- POST Endpoint for Admin to Add New Products ---
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] ProductCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync ();
            try
            {
                // 1. Create the base Product
                var newProduct = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    CategoryId = dto.CategoryId,
                    IsArchived = false // Default to visible
                };
                _context.Products.Add (newProduct);
                await _context.SaveChangesAsync (); // Save to generate the Product ID

                // 2. Add the Base64 Image
                var newImage = new ProductImage
                {
                    ProductId = newProduct.Id,
                    ImageUrl = dto.ImageUrl,
                    IsPrimary = true
                };
                _context.ProductImages.Add (newImage);

                // 3. Add the Variant (Which holds the stock)
                var newVariant = new ProductVariant
                {
                    ProductId = newProduct.Id,
                    Size = "Standard", // You can expand this later to use the dto.Size
                    Color = "Default",
                    StockQuantity = dto.StockQuantity
                };
                _context.ProductVariants.Add (newVariant);

                await _context.SaveChangesAsync ();
                await transaction.CommitAsync ();

                return Ok (newProduct);
            }
            catch ( Exception ex )
            {
                await transaction.RollbackAsync ();
                return StatusCode (500, $"Internal server error: {ex.Message}");
            }
        }

        // --- POST Endpoint for Restocking Inventory ---
        [HttpPost ("{id}/restock")]
        public async Task<IActionResult> RestockProduct(int id, [FromBody] RestockDto dto)
        {
            var variant = await _context.ProductVariants.FirstOrDefaultAsync (v => v.ProductId == id && v.Id == dto.VariantId);

            if ( variant == null ) return NotFound ("Variant not found.");

            // Add the new stock to the existing stock
            variant.StockQuantity += dto.AddQuantity;
            await _context.SaveChangesAsync ();

            return Ok (new { message = "Restocked successfully", newQuantity = variant.StockQuantity });
        }

        // --- PUT Endpoint for Archiving (Soft Delete) ---
        [HttpPut ("{id}/archive")]
        public async Task<IActionResult> ToggleArchive(int id)
        {
            var product = await _context.Products.FindAsync (id);

            if ( product == null ) return NotFound ("Product not found.");

            // Toggle the status (If true, make false. If false, make true.)
            product.IsArchived = !product.IsArchived;
            await _context.SaveChangesAsync ();

            return Ok (new { message = "Archive status updated", isArchived = product.IsArchived });
        }
    }
}