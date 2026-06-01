using Microsoft.EntityFrameworkCore;
using ReserveBag.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL;

var builder = WebApplication.CreateBuilder (args);

// --- ALL BUILDER.SERVICES MUST GO HERE (BEFORE BUILD) ---

// This tells C# to ignore infinite loops when sending data to React
builder.Services.AddControllers ().AddJsonOptions (options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
}); builder.Services.AddEndpointsApiExplorer ();
builder.Services.AddSwaggerGen ();

// 1. Database Connection
builder.Services.AddDbContext<StoreDbContext> (options =>
    options.UseNpgsql (builder.Configuration.GetConnectionString ("DefaultConnection")));
// 2. Setup CORS (Notice this is ABOVE builder.Build)
builder.Services.AddCors (options =>
{
    options.AddPolicy ("AllowReactApp",
        policy =>
        {
            policy.AllowAnyOrigin()   
                  .AllowAnyHeader ()
                  .AllowAnyMethod ();
        });
});
var key = Encoding.ASCII.GetBytes ("ThisIsAVerySecretKeyForJwtAuthentication12345!");

builder.Services.AddAuthentication (options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer (options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey (key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Run the background cleanup job automatically
builder.Services.AddHostedService<ReserveBag.Services.ReservationCleanupService> ();

var app = builder.Build ();

if ( app.Environment.IsDevelopment () )
{
    app.UseSwagger ();
    app.UseSwaggerUI ();
}

app.UseHttpsRedirection ();

// 3. Enable CORS
app.UseCors ("AllowReactApp");

// The order is critical here!
app.UseAuthentication ();
app.UseAuthorization ();

app.MapControllers ();

app.Run ();