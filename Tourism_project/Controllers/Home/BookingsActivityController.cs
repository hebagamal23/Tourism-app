using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism_project.Models;

namespace Tourism_project.Controllers.Home
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsActivityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BookingsActivityController(ApplicationDbContext dbContext)
        {
            this._context = dbContext;
        }

        //    [HttpPost("book-activities/{userId}/{bookingId}")]
        //    public async Task<IActionResult> BookActivities(int userId, int bookingId)
        //    {
        //        try
        //        {
        //            // تحقق من أن الحجز موجود فعلاً
        //            var booking = await _context.bookings.FindAsync(bookingId);
        //            if (booking == null)
        //            {
        //                return NotFound(new { StatusCode = 404, Message = "Booking not found." });
        //            }

        //            // جلب الأنشطة من السلة
        //            var cartItems = await _context.AddActivityToCarts
        //                .Where(c => c.UserId == userId)
        //                .ToListAsync();

        //            if (!cartItems.Any())
        //            {
        //                return BadRequest(new { StatusCode = 400, Message = "No activities in cart." });
        //            }

        //            // إضافة الأنشطة إلى جدول الحجز
        //            foreach (var item in cartItems)
        //            {
        //                var bookingActivity = new BookingActivity
        //                {
        //                    BookingId = bookingId,
        //                    ActivityId = item.ActivityId
        //                };

        //                _context.BookingActivities.Add(bookingActivity);
        //            }

        //            // حذف الأنشطة من السلة بعد إضافتها للحجز (اختياري)
        //            _context.AddActivityToCarts.RemoveRange(cartItems);

        //            await _context.SaveChangesAsync();

        //            return Ok(new
        //            {
        //                StatusCode = 200,
        //                Message = "Activities booked successfully."
        //            });
        //        }
        //        catch (Exception ex)
        //        {
        //            return StatusCode(500, new
        //            {
        //                StatusCode = 500,
        //                Message = "An error occurred while booking activities.",
        //                Details = ex.Message
        //            });
        //        }
        //    }

        //

        [HttpPost("confirm-activity-booking")]
        public async Task<IActionResult> ConfirmActivityBooking(int userId)
        {
            try
            {
                // 0. التحقق من وجود المستخدم
                var userExists = await _context.users.AnyAsync(u => u.TouristId == userId);
                if (!userExists)
                {
                    return NotFound(new
                    {
                        StatusCode = 404,
                        Message = "User not found."
                    });
                }

                // 1. التحقق من وجود أنشطة في السلة
                var cartActivities = await _context.AddActivityToCarts
                    .Where(x => x.UserId == userId)
                    .ToListAsync();

                if (!cartActivities.Any())
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "No activities in the cart."
                    });
                }

                // 2. إنشاء Booking جديد خاص بالأنشطة فقط
                var bookingToUse = new Booking
                {
                    TouristId = userId,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(1), // أو وقت النشاط لو معروف
                    Status = Booking.BookingStatus.Confirmed,
                    RoomId = null, // لا يوجد غرفة
                    TotalPrice = 0, // احسب السعر إن وُجد
                    NumberOfGuests = 1,
                    PaymentMethodId = 1 // لو مفيش دفع
                };

                _context.bookings.Add(bookingToUse);
                await _context.SaveChangesAsync(); // لازم تحفظ عشان تاخد BookingId

                // 3. ربط الأنشطة بالحجز
                foreach (var item in cartActivities)
                {
                    var bookingActivity = new BookingActivity
                    {
                        BookingId = bookingToUse.BookingId,
                        ActivityId = item.ActivityId
                    };
                    _context.BookingActivities.Add(bookingActivity);
                }

                // 4. حذف السلة
                _context.AddActivityToCarts.RemoveRange(cartActivities);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Activities booked successfully.",
                    BookingId = bookingToUse.BookingId
                });
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An error occurred while booking the activities.",
                    Error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

    }
}
