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

        //[HttpPost("confirm-activity-booking")]
        //public async Task<IActionResult> ConfirmActivityBooking(int userId)
        //{
        //    try
        //    {
        //        // 0. التحقق من وجود المستخدم
        //        var userExists = await _context.users.AnyAsync(u => u.TouristId == userId);
        //        if (!userExists)
        //        {
        //            return NotFound(new
        //            {
        //                StatusCode = 404,
        //                Message = "User not found."
        //            });
        //        }

        //        // 1. التحقق من وجود أنشطة في السلة
        //        var cartActivities = await _context.AddActivityToCarts
        //            .Where(x => x.UserId == userId)
        //            .ToListAsync();

        //        if (!cartActivities.Any())
        //        {
        //            return BadRequest(new
        //            {
        //                StatusCode = 400,
        //                Message = "No activities in the cart."
        //            });
        //        }

        //        // 2. إنشاء Booking جديد خاص بالأنشطة فقط
        //        var bookingToUse = new Booking
        //        {
        //            TouristId = userId,
        //            StartDate = DateTime.Now,
        //            EndDate = DateTime.Now.AddDays(1), // أو وقت النشاط لو معروف
        //            Status = Booking.BookingStatus.Confirmed,
        //            RoomId = null, // لا يوجد غرفة
        //            TotalPrice = 0, // احسب السعر إن وُجد
        //            NumberOfGuests = 1,
        //            PaymentMethodId = 1 // لو مفيش دفع
        //        };

        //        _context.bookings.Add(bookingToUse);
        //        await _context.SaveChangesAsync(); // لازم تحفظ عشان تاخد BookingId

        //        // 3. ربط الأنشطة بالحجز
        //        foreach (var item in cartActivities)
        //        {
        //            var bookingActivity = new BookingActivity
        //            {
        //                BookingId = bookingToUse.BookingId,
        //                ActivityId = item.ActivityId
        //            };
        //            _context.BookingActivities.Add(bookingActivity);
        //        }

        //        // 4. حذف السلة
        //        _context.AddActivityToCarts.RemoveRange(cartActivities);
        //        await _context.SaveChangesAsync();

        //        return Ok(new
        //        {
        //            StatusCode = 200,
        //            Message = "Activities booked successfully.",
        //            BookingId = bookingToUse.BookingId
        //        });
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            StatusCode = 500,
        //            Message = "An error occurred while booking the activities.",
        //            Error = ex.InnerException?.Message ?? ex.Message
        //        });
        //    }
        //}


        [HttpPost("confirm-activity-booking")]
        public async Task<IActionResult> ConfirmActivityBooking(int userId)
        {
            try
            {
                // 1. التحقق من وجود المستخدم
                var user = await _context.users
                    .Include(u => u.Bookings)
                        .ThenInclude(b => b.Room)
                            .ThenInclude(r => r.Hotel)
                                .ThenInclude(h => h.Location)
                    .FirstOrDefaultAsync(u => u.TouristId == userId);

                if (user == null)
                {
                    return NotFound(new { StatusCode = 404, Message = "User not found." });
                }

                // 2. التحقق من وجود أنشطة في السلة
                var cartActivities = await _context.AddActivityToCarts
                    .Include(a => a.Activity)
                        .ThenInclude(ac => ac.locationActivities)
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

                // 3. التحقق من أن لديه حجز غرفة مؤكد
                var confirmedRoomBooking = user.Bookings
                    .FirstOrDefault(b => b.RoomId != null && b.Status == Booking.BookingStatus.Confirmed);

                if (confirmedRoomBooking == null)
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "You must book a room before booking activities."
                    });
                }

                var roomLocationId = confirmedRoomBooking.Room.Hotel.LocationId;

                // 4. التحقق من أن كل الأنشطة بنفس موقع الغرفة
                var mismatchedActivities = cartActivities
                    .Where(a => !a.Activity.locationActivities.Any(la => la.LocationId == roomLocationId))
                    .ToList();

                if (mismatchedActivities.Any())
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "All selected activities must be in the same location as your booked room."
                    });
                }

                // 5. إنشاء حجز جديد وربط الأنشطة به
                var bookingToUse = new Booking
                {
                    TouristId = userId,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(1),
                    Status = Booking.BookingStatus.Confirmed,
                    RoomId = null,
                    TotalPrice = cartActivities.Sum(a => a.NumberOfGuests * (decimal)a.Activity.Price),
                    NumberOfGuests = cartActivities.Sum(a => a.NumberOfGuests),
                    PaymentMethodId = 1
                };

                _context.bookings.Add(bookingToUse);
                await _context.SaveChangesAsync();

                foreach (var item in cartActivities)
                {
                    var bookingActivity = new BookingActivity
                    {
                        BookingId = bookingToUse.BookingId,
                        ActivityId = item.ActivityId
                    };
                    _context.BookingActivities.Add(bookingActivity);
                }

                // 6. حذف السلة
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


        [HttpGet("activity-bookings/{userId}")]
        public async Task<ActionResult> GetActivityBookings(int userId)
        {
            try
            {
                var user = await _context.users
                    .Include(u => u.AspNetUser)
                    .FirstOrDefaultAsync(u => u.TouristId == userId);

                if (user == null)
                {
                    return NotFound(new { StatusCode = 404, message = "User not found." });
                }

                var activityBookings = await _context.bookings
                    .Where(b => b.TouristId == userId && b.RoomId == null)
                    .Include(b => b.BookingActivities)
                        .ThenInclude(ba => ba.Activity)
                    .ToListAsync();

                if (!activityBookings.Any())
                {
                    return NotFound(new { StatusCode = 404, message = "No activity bookings found." });
                }

                var result = activityBookings.Select(b => new
                {
                    BookingId = b.BookingId,
                    Status = b.Status.ToString(),
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    NumberOfGuests = b.NumberOfGuests,
                    Activities = b.BookingActivities.Select(ba => new
                    {
                        ActivityId = ba.Activity.ActivityId,
                        ActivityName = ba.Activity.Name,
                        Price = ba.Activity.Price
                    }).ToList()
                }).ToList();

                decimal total = result.Sum(b =>
                     b.Activities.Sum(a => (decimal)a.Price * b.NumberOfGuests)
                 );

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Activity bookings retrieved successfully.",
                    UserInfo = new
                    {
                        FullName = user.AspNetUser.UserName,
                        Email = user.AspNetUser.Email
                    },
                    TotalActivityBookingsPrice = total,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    message = "An error occurred while retrieving activity bookings.",
                    details = ex.Message
                });
            }
        }



    }
}
