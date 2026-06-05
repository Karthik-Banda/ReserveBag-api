using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReserveBag.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReserveBag.Services
{
    public class ReservationCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservationCleanupService> _logger;

        // Inject the ScopeFactory instead of the DbContext directly
        public ReservationCleanupService(IServiceScopeFactory scopeFactory, ILogger<ReservationCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation ("Reservation Cleanup Service is starting.");

            // Keep running until the application shuts down
            while ( !stoppingToken.IsCancellationRequested )
            {
                try
                {
                    // Create a fresh scope for the database context
                    using ( var scope = _scopeFactory.CreateScope () )
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext> ();

                        // Find all pending reservations that have passed their expiration date
                        var expiredReservations = await dbContext.Reservations
                            .Where (r => r.Status == "Pending" && r.ExpiresAt <= DateTime.UtcNow)
                            .ToListAsync (stoppingToken);

                        if ( expiredReservations.Any () )
                        {
                            foreach ( var reservation in expiredReservations )
                            {
                                reservation.Status = "Cancelled";
                            }

                            await dbContext.SaveChangesAsync (stoppingToken);
                            _logger.LogInformation ($"Successfully cancelled {expiredReservations.Count} expired reservations.");
                        }
                    }
                }
                catch ( Exception ex )
                {
                    // Catch the error and log it so it DOES NOT crash the entire application!
                    _logger.LogError (ex, "An error occurred while cleaning up reservations. Retrying next cycle.");
                }

                // Wait for 1 hour before checking the database again
                await Task.Delay (TimeSpan.FromHours (1), stoppingToken);
            }
        }
    }
}