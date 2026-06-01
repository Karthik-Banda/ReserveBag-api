using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ReserveBag.Data;

namespace ReserveBag.Services
{
    public class ReservationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public ReservationCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while ( !stoppingToken.IsCancellationRequested )
            {
                using ( var scope = _serviceProvider.CreateScope () )
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<StoreDbContext> ();

                    // Find all pending reservations older than 48 hours
                    var expiredDate = DateTime.UtcNow.AddHours (-48);

                    var expiredReservations = await dbContext.Reservations
                        .Include (r => r.Items)
                        .ThenInclude (i => i.Variant)
                        .Where (r => r.Status == "Pending" && r.CreatedAt < expiredDate)
                        .ToListAsync (stoppingToken);

                    foreach ( var reservation in expiredReservations )
                    {
                        // 1. Mark as expired
                        reservation.Status = "Expired";

                        // 2. Restore the stock quantities
                        foreach ( var item in reservation.Items )
                        {
                            if ( item.Variant != null )
                            {
                                item.Variant.StockQuantity += item.Quantity;
                            }
                        }
                    }

                    if ( expiredReservations.Any () )
                    {
                        await dbContext.SaveChangesAsync (stoppingToken);
                    }
                }

                // Wait 1 hour before checking again
                await Task.Delay (TimeSpan.FromHours (1), stoppingToken);
            }
        }
    }
}