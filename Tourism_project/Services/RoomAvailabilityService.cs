using Microsoft.EntityFrameworkCore;
using Tourism_project.Models;

namespace Tourism_project.Services
{
    public class RoomAvailabilityService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RoomAvailabilityService> _logger;

        public RoomAvailabilityService(IServiceScopeFactory scopeFactory, ILogger<RoomAvailabilityService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    try
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var now = DateTime.Now;

                        var roomsToUpdate = await dbContext.bookings
                            .Include(b => b.Room)
                            .Where(b => b.StartDate.Date == now.Date &&
                                        b.StartDate <= now && // بدأ الحجز فعلاً
                                        b.Room.IsAvailable == true)
                            .Select(b => b.Room)
                            .ToListAsync();

                        foreach (var room in roomsToUpdate)
                        {
                            room.IsAvailable = false;
                            _logger.LogInformation($"🚫 تم تحديث الغرفة {room.Id} إلى غير متاحة.");
                        }

                        if (roomsToUpdate.Any())
                        {
                            await dbContext.SaveChangesAsync();
                            _logger.LogInformation($"✅ تم حفظ التحديثات لـ {roomsToUpdate.Count} غرفة.");
                        }
                        else
                        {
                            _logger.LogInformation("ℹ️ لا توجد غرف بحاجة إلى تحديث.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"⚠️ خطأ في خدمة توافر الغرف: {ex.Message}");
                    }
                }

                // تشغيل الخدمة كل ساعتين
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

}
