using Tourism_project.Models;

namespace Tourism_project.Services
{
    public class BookingCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;

        public BookingCleanupService(IServiceProvider services)
        {
            _services = services;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var threshold = DateTime.Now.AddMinutes(-5);

                        var oldBookings = context.bookings
                            .Where(b => b.Status == Booking.BookingStatus.Pending && b.CreatedAt < threshold)
                            .ToList(); // تأكد إنك منفذ الاستعلام

                        if (oldBookings.Any())
                        {
                            context.bookings.RemoveRange(oldBookings);
                            await context.SaveChangesAsync();
                            Console.WriteLine($"Removed {oldBookings.Count} old bookings.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"BookingCleanupService error: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

    }

}
