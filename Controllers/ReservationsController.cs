using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReserveBag.Data;
using ReserveBag.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReserveBag.Controllers
{
    [Route ("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly StoreDbContext _context;

        public ReservationsController(StoreDbContext context)
        {
            _context = context;
        }

        // POST: api/reservations
        [HttpPost]
        public async Task<ActionResult<Reservation>> CreateReservation(Reservation reservationRequest)
        {
            if ( reservationRequest.Items == null || !reservationRequest.Items.Any () )
            {
                return BadRequest ("Reserve bag is empty.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync ();
            try
            {
                foreach ( var item in reservationRequest.Items )
                {
                    var variant = await _context.ProductVariants.FindAsync (item.VariantId);
                    if ( variant == null ) return NotFound ($"Variant {item.VariantId} not found.");

                    if ( variant.StockQuantity < item.Quantity )
                    {
                        return BadRequest ($"Not enough stock for Variant ID: {item.VariantId}. Available: {variant.StockQuantity}");
                    }

                    variant.StockQuantity -= item.Quantity;
                }

                _context.Reservations.Add (reservationRequest);
                await _context.SaveChangesAsync ();
                await transaction.CommitAsync ();

                return CreatedAtAction (nameof (GetReservation), new { id = reservationRequest.Id }, reservationRequest);
            }
            catch ( Exception )
            {
                await transaction.RollbackAsync ();
                return StatusCode (500, "An error occurred while saving the reservation.");
            }
        }

        // GET: api/reservations/5
        [HttpGet ("{id}")]
        public async Task<ActionResult<Reservation>> GetReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include (r => r.Items)
                .ThenInclude (i => i.Variant)
                .ThenInclude (v => v.Product)
                .FirstOrDefaultAsync (r => r.Id == id);

            if ( reservation == null ) return NotFound ();
            return Ok (reservation);
        }

        // --- NEW: GET CUSTOMER'S PREVIOUS ORDERS ---
        // GET: api/reservations/customer/6281037543
        [HttpGet ("customer/{phone}")]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetCustomerReservations(string phone)
        {
            var reservations = await _context.Reservations
                .Include (r => r.Items)
                .ThenInclude (i => i.Variant)
                .ThenInclude (v => v.Product)
                .ThenInclude (p => p.Images)
                .Where (r => r.CustomerPhone == phone) // Filter by the user's phone number
                .OrderByDescending (r => r.CreatedAt)
                .AsSplitQuery ()
                .ToListAsync ();

            return Ok (reservations);
        }

        // GET: api/reservations (For Admin Dashboard)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reservation>>> GetReservations()
        {
            return await _context.Reservations
                .Include (r => r.Items)
                .ThenInclude (i => i.Variant)
                .ThenInclude (v => v.Product)
                .ThenInclude (p => p.Images)
                .OrderByDescending (r => r.CreatedAt)
                .AsSplitQuery ()
                .ToListAsync ();
        }

        // PUT: api/reservations/5/status (For Admin Dashboard)
        [HttpPut ("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var reservation = await _context.Reservations
                .Include (r => r.Items)
                .ThenInclude (i => i.Variant)
                .FirstOrDefaultAsync (r => r.Id == id);

            if ( reservation == null ) return NotFound ();

            // Restore stock if the order is cancelled!
            if ( status == "Cancelled" && reservation.Status != "Cancelled" )
            {
                foreach ( var item in reservation.Items )
                {
                    if ( item.Variant != null )
                    {
                        item.Variant.StockQuantity += item.Quantity;
                    }
                }
            }

            reservation.Status = status;
            await _context.SaveChangesAsync ();
            return NoContent ();
        }
    }
}