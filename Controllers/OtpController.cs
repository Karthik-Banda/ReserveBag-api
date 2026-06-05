using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using ReserveBag.Services;
using System;
using System.Threading.Tasks;

namespace ReserveBag.Controllers
{
    [Route ("api/[controller]")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly ISmsService _smsService;
        private readonly IMemoryCache _cache;

        public OtpController(ISmsService smsService, IMemoryCache cache)
        {
            _smsService = smsService;
            _cache = cache;
        }

        [HttpPost ("send")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            if ( string.IsNullOrWhiteSpace (request.Phone) )
                return BadRequest (new { message = "Phone number is required." });

            // Generate a random 6-digit OTP
            var otp = new Random ().Next (100000, 999999).ToString ();

            // Store the OTP in memory cache for 5 minutes
            _cache.Set (request.Phone, otp, TimeSpan.FromMinutes (5));

            // Send the SMS
            var message = $"Your ReserveBag verification code is {otp}. It expires in 5 minutes.";

            try
            {
                await _smsService.SendSmsAsync (request.Phone, message);
                return Ok (new { message = "OTP sent successfully." });
            }
            catch ( Exception ex )
            {
                // Catch Twilio errors (like unverified numbers in trial mode)
                return StatusCode (500, new { message = $"Failed to send SMS: {ex.Message}" });
            }
        }

        [HttpPost ("verify")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            // Check if the OTP exists in cache for this phone number
            if ( _cache.TryGetValue (request.Phone, out string storedOtp) )
            {
                if ( storedOtp == request.Code )
                {
                    // Clean up the cache so it can't be reused
                    _cache.Remove (request.Phone);

                    return Ok (new { success = true, message = "OTP verified successfully." });
                }
            }

            return BadRequest (new { success = false, message = "Invalid or expired OTP." });
        }
    }

    // DTOs for the requests
    public class SendOtpRequest
    {
        public string Phone { get; set; }
    }

    public class VerifyOtpRequest
    {
        public string Phone { get; set; }
        public string Code { get; set; }
    }
}