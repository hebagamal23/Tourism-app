using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism_project.Dtos.Home;
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

       
        #region EndPoint_prepare-activity-booking
        [HttpPost("prepare-activity-booking")]
        public async Task<IActionResult> PrepareActivityBooking(int userId)
        {
            try
            {
                // جلب المستخدم بناءً على الـ userId المرسل
                var user = await _context.users
                    .Include(u => u.Bookings)
                        .ThenInclude(b => b.Room)
                            .ThenInclude(r => r.Hotel)
                                .ThenInclude(h => h.Location)
                    .FirstOrDefaultAsync(u => u.TouristId == userId);

                if (user == null)
                    return NotFound(new { StatusCode = 404, Message = "User not found." });

                // جلب الأنشطة الموجودة في سلة المشتريات للمستخدم
                var cartActivities = await _context.AddActivityToCarts
                    .Include(a => a.Activity)
                        .ThenInclude(ac => ac.locationActivities)
                    .Where(x => x.UserId == userId)
                    .ToListAsync();

                // إذا كانت سلة المشتريات فارغة، نرجع رسالة خطأ
                if (!cartActivities.Any())
                    return BadRequest(new { StatusCode = 400, Message = "No activities in the cart." });

                // محاولة العثور على حجز غرفة مؤكد للمستخدم
                var confirmedRoomBooking = user.Bookings
                    .FirstOrDefault(b => b.RoomId != null && b.Status == Booking.BookingStatus.Confirmed);

                DateTime? startDate = null;
                DateTime? endDate = null;
                int? roomLocationId = null;

                // إذا كان هناك حجز غرفة مؤكد، نعين التواريخ ونحقق في الموقع
                if (confirmedRoomBooking != null)
                {
                    startDate = confirmedRoomBooking.StartDate;
                    endDate = confirmedRoomBooking.EndDate;
                    roomLocationId = confirmedRoomBooking.Room.Hotel.LocationId;

                    // التأكد من أن الأنشطة في نفس الموقع كما في الحجز
                    var mismatchedActivities = cartActivities
                        .Where(a => !a.Activity.locationActivities.Any(la => la.LocationId == roomLocationId))
                        .ToList();

                    if (mismatchedActivities.Any())
                        return BadRequest(new { StatusCode = 400, Message = "All selected activities must be in the same location as your booked room." });
                }
                else
                {
                    // إذا لم يكن هناك حجز غرفة مؤكد، نسمح للمستخدم بالمتابعة بدون غرفة
                    startDate = DateTime.Now; // بداية افتراضية (مثلاً اليوم الحالي)
                 
                }

                // 🟡 التحقق من الحجز المعلق
                var hasPendingBooking = user.Bookings.Any(b => b.Status == Booking.BookingStatus.Pending);
                if (hasPendingBooking)
                {
                    return BadRequest(new { StatusCode = 400, Message = "You already have a pending booking. Please complete or cancel it before creating a new one." });
                } 

                // إنشاء حجز جديد بناءً على الأنشطة في سلة المشتريات
                var newBooking = new Booking
                {
                    TouristId = userId,
                    RoomId = null, // ليس لدينا غرفة هنا
                    PaymentMethodId = 1, // تحديد طريقة الدفع الافتراضية
                    StartDate = startDate ?? DateTime.MinValue, // استخدام التاريخ المحدد أو الحد الأدنى للتاريخ
                    EndDate = endDate ?? DateTime.MinValue, // استخدام التاريخ المحدد أو الحد الأدنى للتاريخ
                    TotalPrice = cartActivities.Sum(a => a.NumberOfGuests * (decimal)a.Activity.Price), // إجمالي السعر بناءً على الأنشطة وعدد الضيوف
                    NumberOfGuests = cartActivities.Sum(a => a.NumberOfGuests), // إجمالي عدد الضيوف
                    Status = Booking.BookingStatus.Pending // حالة الحجز مبدئية كـ "معلق"
                };

                // إضافة الحجز الجديد إلى قاعدة البيانات
                _context.bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                // إرجاع استجابة تفيد بأنه تم إعداد الحجز
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Activity booking prepared. Proceed to payment.",
                    BookingId = newBooking.BookingId,
                    HasRoom = confirmedRoomBooking != null // نُعلم المستخدم إذا كان لديه غرفة مؤكدَّة أم لا
                });
            }
            catch (Exception ex)
            {
                // في حالة حدوث خطأ، نرجع رسالة خطأ
                return StatusCode(500, new { StatusCode = 500, Message = "Error preparing booking", Error = ex.Message });
            }
        }
        #endregion


        #region EndPoint_get-activities-for-booking
        [HttpGet("get-activities-for-booking")]
        public async Task<IActionResult> GetActivitiesForBooking(int userId)
        {
            try
            {
                // جلب المستخدم بناءً على الـ userId المرسل
                var user = await _context.users
                    .Include(u => u.Bookings)
                        .ThenInclude(b => b.Room)
                            .ThenInclude(r => r.Hotel)
                                .ThenInclude(h => h.Location)
                    .FirstOrDefaultAsync(u => u.TouristId == userId);

                if (user == null)
                    return NotFound(new { StatusCode = 404, Message = "User not found." });

                // جلب الأنشطة الموجودة في سلة المشتريات للمستخدم
                var cartActivities = await _context.AddActivityToCarts
                    .Include(a => a.Activity)
                        .ThenInclude(ac => ac.locationActivities)
                    .Where(x => x.UserId == userId)
                    .ToListAsync();

                // إذا كانت سلة المشتريات فارغة، نرجع رسالة خطأ
                if (!cartActivities.Any())
                    return BadRequest(new { StatusCode = 400, Message = "No activities in the cart." });

                // محاولة العثور على حجز غرفة مؤكد للمستخدم
                var confirmedRoomBooking = user.Bookings
                    .FirstOrDefault(b => b.RoomId != null && b.Status == Booking.BookingStatus.Confirmed);

                if (confirmedRoomBooking != null)
                {
                    // إذا كان يوجد حجز غرفة مؤكد، نعرض الأنشطة مع التواريخ الخاصة بالحجز
                    var activitiesWithDates = cartActivities.Select(a => new
                    {
                        ActivityId = a.ActivityId,
                        ActivityName = a.Activity.Name,
                        ActivityPrice = a.Activity.Price,
                        ActivityImageUrl = a.Activity.ImageUrl,
                        ActivityStartDate = confirmedRoomBooking.StartDate,
                        ActivityEndDate = confirmedRoomBooking.EndDate,
                        NumberOfGuests = a.NumberOfGuests
                    });

                    return Ok(new
                    {
                        StatusCode = 200,
                        Message = "Activities retrieved successfully with booking dates.",
                        Activities = activitiesWithDates
                    });
                }
                else
                {
                    // إذا لم يكن هناك حجز غرفة مؤكد، نعرض الأنشطة بدون تواريخ
                    var activitiesWithoutDates = cartActivities.Select(a => new
                    {
                        ActivityId = a.ActivityId,
                        ActivityName = a.Activity.Name,
                        ActivityPrice = a.Activity.Price,
                        ActivityImageUrl = a.Activity.ImageUrl,
                        NumberOfGuests = a.NumberOfGuests
                    });

                    return Ok(new
                    {
                        StatusCode = 200,
                        Message = "Activities retrieved successfully without booking dates.",
                        Activities = activitiesWithoutDates
                    });
                }
            }
            catch (Exception ex)
            {
                // في حالة حدوث خطأ، نرجع رسالة خطأ
                return StatusCode(500, new { StatusCode = 500, Message = "Error retrieving activities", Error = ex.Message });
            }
        }
        #endregion

        #region EndPoint_confirm-activity-booking
        [HttpPost("confirm-activity-booking")]
        public async Task<IActionResult> ConfirmActivityBooking(int userId, List<ActivityBookingRequestDTO> activityBookings)
        {
            try
            {
                // التحقق من وجود المستخدم
                var user = await _context.users.FirstOrDefaultAsync(u => u.TouristId == userId);
                if (user == null)
                    return NotFound(new { StatusCode = 404, Message = "User not found." });

                // جلب الأنشطة في سلة التسوق
                var cartItems = await _context.AddActivityToCarts
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                if (cartItems == null || !cartItems.Any())
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "Your cart is empty. Please add activities before confirming the booking."
                    });
                }

                // التحقق من وجود حجز قيد الانتظار
                var existingBooking = await _context.bookings
                    .Include(b => b.Payment)
                    .FirstOrDefaultAsync(b => b.TouristId == userId && b.Status == Booking.BookingStatus.Pending);

                if (existingBooking == null)
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = "You don't have a pending booking to confirm. Please create a booking first."
                    });
                }

                // التحقق أن كل نشاط في السلة موجود في الطلب
                var cartActivityIds = cartItems.Select(c => c.ActivityId).Distinct().ToList();
                var requestActivityIds = activityBookings.Select(ab => ab.ActivityId).Distinct().ToList();

                var missingActivities = cartActivityIds.Except(requestActivityIds).ToList();
                if (missingActivities.Any())
                {
                    return BadRequest(new
                    {
                        StatusCode = 400,
                        Message = $"Missing activity data in your booking request for activity IDs: {string.Join(", ", missingActivities)}. Please provide data for all activities in your cart."
                    });
                } 


                // تحديث حالة الحجز إلى "مؤكد"
                existingBooking.Status = Booking.BookingStatus.Confirmed;
                existingBooking.StartDate = activityBookings.Min(ab => ab.StartDate);
                existingBooking.EndDate = activityBookings.Max(ab => ab.StartDate);
                existingBooking.TotalPrice = activityBookings.Sum(ab => ab.NumberOfGuests * (decimal)ab.ActivityPrice);
                existingBooking.NumberOfGuests = activityBookings.Sum(ab => ab.NumberOfGuests);
                existingBooking.PaymentTime = DateTime.UtcNow;

                // إضافة الأنشطة المحجوزة إلى الحجز مع التحقق من التواريخ
                // جلب حجز الغرفة المؤكد إن وجد
                var confirmedRoomBooking = await _context.bookings
                    .FirstOrDefaultAsync(b =>
                        b.TouristId == userId &&
                        b.Status == Booking.BookingStatus.Confirmed &&
                        b.RoomId != null
                    );

                // لكل نشاط في الـ DTO
                foreach (var ab in activityBookings)
                {
                    // أولًا: تحقق أن المستخدم فعلاً أضاف هذا النشاط للسلة
                    var cartItem = cartItems.FirstOrDefault(c => c.ActivityId == ab.ActivityId);
                    if (cartItem == null)
                        return BadRequest(new
                        {
                            StatusCode = 400,
                            Message = $"Activity with ID {ab.ActivityId} was not added to the cart."
                        });

                    // تحقق من تطابق السعر
                    if (cartItem.ActivityPrice != ab.ActivityPrice)
                    {
                        return BadRequest(new
                        {
                            StatusCode = 400,
                            Message = $"Price for activity {ab.ActivityId} does not match the price in the cart."
                        });
                    }

                    // تحقق من تطابق عدد الضيوف
                    if (cartItem.NumberOfGuests != ab.NumberOfGuests)
                    {
                        return BadRequest(new
                        {
                            StatusCode = 400,
                            Message = $"Number of guests for activity {ab.ActivityId} does not match the number in the cart."
                        });
                    }

                    // تحقق من تطابق اسم النشاط في السلة وفي الـ DTO
                    if (cartItem.ActivityName.Trim().ToLower() != ab.ActivityName.Trim().ToLower())
                    {
                        return BadRequest(new
                        {
                            StatusCode = 400,
                            Message = $"Activity name for ID {ab.ActivityId} does not match the name in the cart. Expected: '{cartItem.ActivityName}', Provided: '{ab.ActivityName}'."
                        });
                    }


                    // خذ التاريخ من الـ DTO
                    var requestedDate = ab.StartDate.Date;

                    // لو في حجز غرفة، تحقق من الفترة
                    if (confirmedRoomBooking != null)
                    {
                        if (requestedDate < confirmedRoomBooking.StartDate.Date ||
                            requestedDate > confirmedRoomBooking.EndDate.Date)
                        {
                            return BadRequest(new
                            {
                                StatusCode = 400,
                                Message = $"The activity date {requestedDate.ToShortDateString()} is outside the booking period of the room, which is from {confirmedRoomBooking.StartDate.ToShortDateString()} to {confirmedRoomBooking.EndDate.ToShortDateString()}."
                            });
                        }
                    }

                    // احفظ النشاط مع التاريخ النهائي
                    var bookingActivity = new BookingActivity
                    {
                        BookingId = existingBooking.BookingId,
                        ActivityId = ab.ActivityId,
                        ActivityDate = requestedDate,
                        NumberOfGuests = ab.NumberOfGuests
                    };
                    await _context.BookingActivities.AddAsync(bookingActivity);
                }

                // تحديث الدفع
                var existingPayment = existingBooking.Payment;

                // إذا لم يكن هناك سجل دفع، نضيف سجلاً جديدًا
                if (existingPayment == null)
                {
                    var newPayment = new Payment
                    {
                        BookingId = existingBooking.BookingId,
                        PaymentTime = DateTime.UtcNow,
                        Amount = existingBooking.TotalPrice,
                        PaymentMethodId = existingBooking.PaymentMethodId, // تأكد من أن PaymentMethodId موجود
                        Status = "Completed" // حالة الدفع هي مكتملة بعد تأكيد الحجز
                    };
                    await _context.Payments.AddAsync(newPayment);
                }
                else
                {
                    // إذا كان هناك سجل دفع، نقوم بتحديثه
                    existingPayment.Status = "Completed";
                    existingPayment.PaymentTime = DateTime.UtcNow;
                    existingPayment.Amount = existingBooking.TotalPrice;
                    _context.Payments.Update(existingPayment);
                }

                // إفراغ سلة التسوق وحفظ التغييرات
                _context.AddActivityToCarts.RemoveRange(cartItems);
                _context.bookings.Update(existingBooking);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Activity booking and payment confirmed successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An error occurred while confirming the activity booking.",
                    Error = ex.Message
                });
            }
        }
        #endregion

        #region EndPoint_activity-bookings
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
                        Price = ba.Activity.Price,
                        ActivityDate = ba.ActivityDate
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

        #endregion

    }
}
