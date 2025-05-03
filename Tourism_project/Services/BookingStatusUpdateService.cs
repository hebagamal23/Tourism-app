using Microsoft.EntityFrameworkCore;
using Tourism_project.Models;

namespace Tourism_project.Services
{
    public class BookingStatusUpdateService: BackgroundService
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

                // انتظار فترة معينة (على سبيل المثال 24 ساعة) بين كل تحقق
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

                    // جلب جميع الحجوزات التي انتهت
                    var expiredBookings = await dbContext.bookings
                        .Include(b => b.Room)  // تحميل الغرف المرتبطة بالحجوزات
                        .Where(b => b.EndDate <= DateTime.Now && b.Status != Booking.BookingStatus.Expired)
                        .ToListAsync();

                    if (expiredBookings.Any())
                    {
                        foreach (var booking in expiredBookings)
                        {
                            // تحديث حالة الحجز إلى "منتهية"
                            booking.Status = Booking.BookingStatus.Expired;

                            // تحديث الغرفة إلى "متاحة" إذا كانت غير متاحة
                            if (booking.Room != null && booking.Room.IsAvailable == false)
                            {
                                booking.Room.IsAvailable = true;
                                _logger.LogInformation($"تم تحديث الغرفة {booking.Room.Id} إلى متاحة.");
                            }

                            _logger.LogInformation($"تم تحديث حالة الحجز {booking.BookingId} إلى Expired.");
                        }

                        // حفظ التغييرات مرة واحدة بعد تحديث كل الحجوزات
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"⚠️ خطأ أثناء تحديث الحجوزات المنتهية: {ex.Message}");
            }
        }

    }
}
