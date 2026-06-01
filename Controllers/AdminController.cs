using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ReserveBag.Controllers
{
    [Route ("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        [HttpPost ("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // In a production environment, this should be validated against a database or secure config!
            if ( request.Pin == "1234" )
            {
                var tokenHandler = new JwtSecurityTokenHandler ();
                // This key must be at least 32 characters long
                var key = Encoding.ASCII.GetBytes ("ThisIsAVerySecretKeyForJwtAuthentication12345!");
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity (new [] { new Claim (ClaimTypes.Role, "Admin") }),
                    Expires = DateTime.UtcNow.AddHours (2), // Token is valid for 2 hours
                    SigningCredentials = new SigningCredentials (new SymmetricSecurityKey (key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken (tokenDescriptor);
                return Ok (new { token = tokenHandler.WriteToken (token) });
            }

            return Unauthorized (new { message = "Invalid PIN" });
        }
    }

    public class LoginRequest
    {
        public string Pin { get; set; }
    }
}