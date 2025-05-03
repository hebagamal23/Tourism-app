using Microsoft.EntityFrameworkCore;
using Tourism_project.Models;

namespace Tourism_project.Services
{
    public class RoomAvailabilityService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public RoomAvailabilityService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var today = DateTime.Today;
                    var roomsToUpdate = await dbContext.bookings
                        .Where(b => b.StartDate == today) // يبدأ الحجز اليوم
                        .Select(b => b.Room)
                        .ToListAsync();

                    foreach (var room in roomsToUpdate)
                    {
                        room.IsAvailable = false;
                    }

                    await dbContext.SaveChangesAsync();
                }

                // تشغيل الخدمة مرة واحدة يوميًا
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
    }

}
