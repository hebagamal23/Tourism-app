using Tourism_project.Models;

namespace Tourism_project.Services
{
    public class PaymentCleanupService : BackgroundService
    {

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public PaymentCleanupService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var expirationTime = DateTime.UtcNow.AddMinutes(-5); 

                    var expiredPayments = dbContext.Payments
                        .Where(p => p.Status == "Pending" && p.PaymentTime <= expirationTime)
                        .ToList();

                    foreach (var payment in expiredPayments)
                    {
                        
                        payment.Status = "Expired";

                       
                        var booking = dbContext.bookings.FirstOrDefault(b => b.BookingId == payment.BookingId);
                        if (booking != null)
                        {
                            var room = dbContext.Rooms.FirstOrDefault(r => r.Id == booking.RoomId);
                            if (room != null)
                            {
                                room.IsAvailable = true; 
                            }

                            dbContext.bookings.Remove(booking); 
                        }
                    }

                    await dbContext.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); 
            }
        }
    }
}
