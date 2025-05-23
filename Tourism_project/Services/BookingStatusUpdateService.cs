using Microsoft.EntityFrameworkCore;
using Tourism_project.Models;

namespace Tourism_project.Services
{
    public class BookingStatusUpdateService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<BookingStatusUpdateService> _logger;

        public BookingStatusUpdateService(IServiceScopeFactory serviceScopeFactory, ILogger<BookingStatusUpdateService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateExpiredBookings();
                await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
            }
        }

        private async Task UpdateExpiredBookings()
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var expiredBookings = await dbContext.bookings
                        .Include(b => b.Room)
                        .Where(b => b.EndDate <= DateTime.Now && b.Status != Booking.BookingStatus.Expired)
                        .ToListAsync();

                    if (expiredBookings.Any())
                    {
                        foreach (var booking in expiredBookings)
                        {
                            booking.Status = Booking.BookingStatus.Expired;

                            if (booking.Room != null && booking.Room.IsAvailable == false)
                            {
                                booking.Room.IsAvailable = true;
                                _logger.LogInformation($"Room {booking.Room.Id} is now available.");
                            }

                            _logger.LogInformation($"Booking {booking.BookingId} status updated to Expired.");
                        }

                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error while updating expired bookings: {ex.Message}");
            }
        }
    }
}
