using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism_project.Dtos.Home;
using Tourism_project.Models;
using static Tourism_project.Models.Booking;
using static Tourism_project.Models.Room;

namespace Tourism_project.Controllers.Home
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsRoomsController : ControllerBase
    {

        private readonly ApplicationDbContext dbContext;

        public BookingsRoomsController (ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public static string GetRoomTypeName(RoomType roomType)
        {
            return roomType switch
            {
                RoomType.Single => "Single Room",
                RoomType.Double => "Double Room",
                RoomType.Suite => "Suite",
                RoomType.King_size_Bed => "King size Bed",
                _ => "Unknown" // في حال كانت القيمة غير معروفة
            };
        }




        #region EndPoint_BookRoom

        [HttpPost("book")]
        public async Task<ActionResult> BookRoom([FromBody] BookingDto request)
        {
            try
            {
                // ✅ التحقق من وجود المستخدم
                var userExists = await dbContext.users.AnyAsync(u => u.TouristId == request.UserId);
                if (!userExists)
                {
                    return BadRequest(new { StatusCode = 400, message = "User not found." });
                }

                // ✅ التحقق من وجود الغرفة وتوافرها
                var room = await dbContext.Rooms.FindAsync(request.RoomId);
                if (room == null || !room.IsAvailable)
                {
                    return BadRequest(new { StatusCode = 400, message = "The room is not available for booking." });
                }

                // ✅ التحقق من أن عدد الأفراد لا يتجاوز سعة الغرفة
                if (request.NumberOfGuests > room.MaxOccupancy)
                {
                    return BadRequest(new { StatusCode = 400, message = $"The selected room can only accommodate up to {room.MaxOccupancy} guests." });
                }

                // ✅ التحقق من صحة PaymentMethodId قبل البحث عنه
                if (request.PaymentMethodId <= 0)
                {
                    return BadRequest(new { StatusCode = 400, message = "Invalid Payment Method ID." });
                }

                // ✅ البحث عن طريقة الدفع
                var paymentMethod = await dbContext.paymentMethods.FindAsync(request.PaymentMethodId);
                if (paymentMethod == null)
                {
                    return BadRequest(new { StatusCode = 400, message = "Invalid Payment Method. This payment method does not exist." });
                }

                // ✅ التحقق من أن تاريخ البداية ليس قبل اليوم
                if (request.StartDate < DateTime.Today)
                {
                    return BadRequest(new { StatusCode = 400, message = "Start date cannot be in the past." });
                }

                // ✅ التحقق من أن تاريخ النهاية أكبر من تاريخ البداية
                int numberOfDays = (request.EndDate - request.StartDate).Days;
                if (numberOfDays <= 0)
                {
                    return BadRequest(new { StatusCode = 400, message = "End date must be after start date." });
                }

                // ✅ التحقق من عدم وجود حجوزات متداخلة لنفس الغرفة
                bool isRoomBooked = await dbContext.bookings.AnyAsync(b =>
                    b.RoomId == request.RoomId &&
                    b.Status != Booking.BookingStatus.Cancelled && // ✅ استبعاد الحجوزات الملغاة
                    ((request.StartDate >= b.StartDate && request.StartDate < b.EndDate) || // بداية الحجز الجديد داخل حجز آخر
                    (request.EndDate > b.StartDate && request.EndDate <= b.EndDate) || // نهاية الحجز الجديد داخل حجز آخر
                    (request.StartDate <= b.StartDate && request.EndDate >= b.EndDate)) // الحجز الجديد يغطي حجز آخر بالكامل
                );

                if (isRoomBooked)
                {
                    return BadRequest(new { StatusCode = 400, message = "The room is already booked for the selected dates." });
                }

                // ✅ حساب المبلغ الإجمالي بناءً على عدد الأيام
                decimal totalPrice = room.PricePerNight * numberOfDays;

                // ✅ إنشاء الحجز
                var booking = new Booking
                {
                    RoomId = request.RoomId,
                    TouristId = request.UserId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    TotalPrice = totalPrice,
                    Status = Booking.BookingStatus.Pending,
                    PaymentMethodId = request.PaymentMethodId,
                    NumberOfGuests = request.NumberOfGuests // ✅ تخزين عدد الأفراد في الحجز
                };

                dbContext.bookings.Add(booking);
                await dbContext.SaveChangesAsync(); // 🔹 حفظ الحجز للحصول على `BookingId`

                // ✅ إنشاء معاملة دفع جديدة
                DateTime transactionTime = DateTime.UtcNow;
                DateTime expirationTime = transactionTime.AddMinutes(55); // ⏳ الصلاحية بعد 5 دقائق

                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    PaymentMethodId = request.PaymentMethodId,
                    Amount = booking.TotalPrice,
                    TransactionId = Guid.NewGuid().ToString(),
                    Status = "Pending", // ✅ سيتم تحديثها لاحقًا بعد الدفع
                    PaymentTime = transactionTime
                };

                dbContext.Payments.Add(payment);

                await dbContext.SaveChangesAsync(); // 🔹 حفظ كل التعديلات

                return Ok(new
                {
                    message = "Booking successful! Proceed to payment.",
                    bookingId = booking.BookingId,
                    transactionId = payment.TransactionId,
                    expiresAt = expirationTime, // ⏳ إرسال وقت انتهاء الصلاحية
                    expiresInMinutes = 5 // ⏳ عدد الدقائق حتى انتهاء الصلاحية
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    message = "Internal Server Error",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }


        #endregion

        #region Endpoint_ConfirmPayment

        [HttpPost("confirm-payment/{transactionId}")]
        public async Task<ActionResult> ConfirmPayment(string transactionId)
        {
            try
            {
                // 🔹 البحث عن عملية الدفع بناءً على `transactionId`
                var payment = await dbContext.Payments
                    .Include(p => p.Booking) // ✅ جلب معلومات الحجز أيضًا
                    .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

                if (payment == null)
                {
                    return NotFound(new { StatusCode = 404, message = "Payment transaction not found." });
                }

                // ✅ التحقق مما إذا كانت عملية الدفع قد تمت بالفعل
                if (payment.Status == "Completed")
                {
                    return BadRequest(new { StatusCode = 400, message = "Payment is already completed." });
                }

                // ✅ التحقق من وجود الحجز المرتبط بالدفع
                if (payment.Booking == null)
                {
                    return BadRequest(new { StatusCode = 400, message = "Associated booking not found." });
                }

                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // ✅ تحديث حالة الدفع
                        payment.Status = "Completed";
                        payment.PaymentTime = DateTime.UtcNow; // 🔹 تخزين وقت الدفع
                        dbContext.Payments.Update(payment);

                        // ✅ تحديث حالة الحجز إلى "Confirmed"
                        payment.Booking.Status = Booking.BookingStatus.Confirmed;
                        payment.Booking.PaymentTime = DateTime.UtcNow;
                        dbContext.bookings.Update(payment.Booking);

                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync(); // 🔹 تأكيد العملية

                        return Ok(new
                        {
                            message = "Payment successful! Booking confirmed.",
                            bookingId = payment.BookingId,
                            paymentStatus = payment.Status
                        });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(); // ❌ إلغاء العملية في حال حدوث خطأ
                        return StatusCode(500, new
                        {
                            StatusCode = 500,
                            message = "Internal Server Error",
                            details = ex.InnerException?.Message ?? ex.Message
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    message = "Internal Server Error",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        #endregion

        #region Endpoint_GetBookingDetailsRoomsbeforePayment 
        [HttpGet("booking-details-beforePayment/{bookingId}")]
        public async Task<ActionResult> GetBookingDetails(int bookingId)
        {
            try
            {
                // 🔹 البحث عن الحجز بناءً على `bookingId`
                var booking = await dbContext.bookings
                    .Include(b => b.Room) // ✅ جلب تفاصيل الغرفة المرتبطة بالحجز
                    .Include(b => b.Payment) // ✅ جلب تفاصيل الدفع المرتبطة بالحجز (إذا كانت موجودة)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    return NotFound(new { StatusCode = 404, message = "Booking not found." });
                }

                // 🔹 تحويل `enum` إلى اسم نصي
                string bookingStatusName = Enum.GetName(typeof(BookingStatus), booking.Status);

                // 🔹 تحويل نوع الغرفة إلى نص
                string roomTypeName = GetRoomTypeName((RoomType)booking.Room.Type);

                // 🔹 إعداد بيانات الحجز لعرضها
                var bookingDetails = new
                {
                    bookingId = booking.BookingId,
                    room = new
                    {
                        roomNumber = booking.Room.Id,
                        roomType = roomTypeName,
                        pricePerNight = booking.Room.PricePerNight
                    },
                    startDate = booking.StartDate,
                    endDate = booking.EndDate,
                    totalPrice = booking.TotalPrice,
                    paymentStatus = booking.Payment?.Status ?? "Pending", // إذا كانت عملية الدفع موجودة
                    numberOfGuests = booking.NumberOfGuests,
                    bookingStatus = bookingStatusName // ✅ إرجاع الحالة كنص
                };

                return Ok(new { message = "Booking details retrieved successfully.", bookingDetails });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    message = "Internal Server Error",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        #endregion

        #region Endpoint_GetBookingDetailsAfterPayment 

        [HttpGet("booking-details-AfterPayment/{bookingId}")]
        public async Task<ActionResult> GetBookingDetailsAfterBooking(int bookingId)
        {
            try
            {
                // 🔹 البحث عن الحجز بناءً على `bookingId`
                var booking = await dbContext.bookings
                    .Include(b => b.Room)         // ✅ جلب تفاصيل الغرفة المرتبطة بالحجز
                    .Include(b => b.Payment)      // ✅ جلب تفاصيل الدفع المرتبطة بالحجز
                    .Include(b => b.Tourist)
                    .ThenInclude(t => t.AspNetUser)// ✅ جلب بيانات السائح (المستخدم)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    return NotFound(new { StatusCode = 404, message = "Booking not found." });
                }

                // ✅ التأكد من أن الحجز تم تأكيده
                if (booking.Status != Booking.BookingStatus.Confirmed)
                {
                    return BadRequest(new { StatusCode = 400, message = "Booking is not confirmed yet." });
                }

                // 🔹 تحويل حالة الحجز إلى نص
                string bookingStatusText = booking.Status.ToString();

                // 🔹 تحويل نوع الغرفة إلى نص
                string roomTypeText = booking.Room.Type.ToString();

                // 🔹 إعداد بيانات الحجز لعرضها
                var bookingDetails = new
                {
                    bookingId = booking.BookingId,

                    // ✅ بيانات السائح (المستخدم)
                    user = new
                    {
                        fullName = booking.Tourist.AspNetUser.UserName,
                        passportNumber = booking.Tourist.PassportNumber,
                        email = booking.Tourist.AspNetUser.Email
                    },

                    // ✅ بيانات الغرفة
                    room = new
                    {
                        roomNumber = booking.Room.Id,
                        roomName = booking.Room.Name, // 🔹 اسم الغرفة
                        roomType = roomTypeText, // 🔹 اسم نوع الغرفة بدلًا من رقم الـ Enum
                        pricePerNight = booking.Room.PricePerNight

                    },

                    // ✅ بيانات الحجز
                    startDate = booking.StartDate,
                    endDate = booking.EndDate,
                    totalPrice = booking.TotalPrice,
                    paymentStatus = booking.Payment.Status,
                    paymentTime = booking.Payment.PaymentTime,
                    numberOfGuests = booking.NumberOfGuests,
                    bookingStatus = bookingStatusText // 🔹 عرض حالة الحجز كنص وليس رقم
                };

                return Ok(new { message = "Booking details retrieved successfully.", bookingDetails });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    message = "Internal Server Error",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        #endregion

        //[HttpGet("bookings/{userId}")]
        //public async Task<ActionResult> GetUserBookings(int userId)
        //{
        //    try
        //    {
        //        var userExists = await dbContext.users.AnyAsync(u => u.TouristId == userId);
        //        if (!userExists)
        //        {
        //            return NotFound(new { StatusCode = 404, message = "User not found." });
        //        }

        //        var bookings = await dbContext.bookings
        //            .Where(b => b.TouristId == userId)
        //            .Include(b => b.Room)
        //            .Include(b => b.BookingActivities)
        //                .ThenInclude(ba => ba.Activity)
        //            .ToListAsync();

        //        if (!bookings.Any())
        //        {
        //            return NotFound(new { StatusCode = 404, message = "No bookings found for the user." });
        //        }

        //        var result = bookings.Select(b => new
        //        {
        //            BookingId = b.BookingId,
        //            Status = b.Status.ToString(), // ✅ الحالة كنص
        //            BookingType = b.RoomId.HasValue ? "Room" : (b.BookingActivities.Any() ? "Activity" : "Unknown"),

        //            // ✅ تفاصيل حجز الغرفة فقط إذا كان Room
        //            RoomBooking = b.RoomId.HasValue ? new
        //            {
        //                RoomId = b.Room.Id,
        //                RoomName = b.Room.Name,
        //                RoomPricePerNight = b.Room.PricePerNight,
        //                StartDate = b.StartDate,
        //                EndDate = b.EndDate,
        //                NumberOfNights = (b.EndDate - b.StartDate).Days,
        //                NumberOfGuests = b.NumberOfGuests,
        //            } : null,

        //            // ✅ تفاصيل حجز النشاطات فقط إذا كان Activity
        //            ActivityBookings = (!b.RoomId.HasValue && b.BookingActivities.Any()) ? b.BookingActivities.Select(ba => new
        //            {
        //                ActivityId = ba.Activity.ActivityId,
        //                ActivityName = ba.Activity.Name,
        //                StartDate = b.StartDate,
        //                EndDate = b.EndDate,
        //                NumberOfGuests = b.NumberOfGuests
        //            }).ToList() : null
        //        });

        //        return Ok(new
        //        {
        //            StatusCode = 200,
        //            Message = "Bookings retrieved successfully.",
        //            Data = result
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            StatusCode = 500,
        //            message = "An error occurred while retrieving bookings.",
        //            details = ex.Message
        //        });
        //    }
        //}


        //[HttpGet("bookings/{userId}")]
        //public async Task<ActionResult> GetUserBookings(int userId)
        //{
        //    try
        //    {
        //        var user = await dbContext.users
        //                .Include(u => u.AspNetUser) 
        //                .FirstOrDefaultAsync(u => u.TouristId == userId);
        //        if (user == null)
        //        {
        //            return NotFound(new { StatusCode = 404, message = "User not found." });
        //        }

        //        var bookings = await dbContext.bookings
        //            .Where(b => b.TouristId == userId)
        //            .Include(b => b.Room)
        //            .Include(b => b.BookingActivities)
        //                .ThenInclude(ba => ba.Activity)
        //            .ToListAsync();

        //        if (!bookings.Any())
        //        {
        //            return NotFound(new { StatusCode = 404, message = "No bookings found for the user." });
        //        }

        //        // قائمة الحجوزات النهائية
        //        var bookingList = bookings.Select(b => new
        //        {
        //            BookingId = b.BookingId,
        //            Status = b.Status.ToString(),
        //            BookingType = b.RoomId.HasValue ? "Room" : (b.BookingActivities.Any() ? "Activity" : "Unknown"),

        //            RoomBooking = b.RoomId.HasValue ? new
        //            {
        //                RoomId = b.Room.Id,
        //                RoomName = b.Room.Name,
        //                RoomPricePerNight = b.Room.PricePerNight,
        //                StartDate = b.StartDate,
        //                EndDate = b.EndDate,
        //                NumberOfNights = (b.EndDate - b.StartDate).Days,
        //                NumberOfGuests = b.NumberOfGuests,
        //                TotalPrice = b.Room.PricePerNight * (b.EndDate - b.StartDate).Days
        //            } : null,

        //            ActivityBookings = (!b.RoomId.HasValue && b.BookingActivities.Any()) ? b.BookingActivities.Select(ba => new
        //            {
        //                ActivityId = ba.Activity.ActivityId,
        //                ActivityName = ba.Activity.Name,
        //                Price = ba.Activity.Price,
        //                StartDate = b.StartDate,
        //                EndDate = b.EndDate,
        //                NumberOfGuests = b.NumberOfGuests
        //            }).ToList() : null
        //        }).ToList();

        //        // حساب الإجماليات
        //        decimal totalRoomsPrice = bookingList
        //            .Where(b => b.RoomBooking != null)
        //            .Sum(b => (decimal)b.RoomBooking.TotalPrice);

        //        decimal totalActivitiesPrice = bookingList
        //            .Where(b => b.ActivityBookings != null)
        //            .SelectMany(b => b.ActivityBookings)
        //            .Sum(a => (decimal)a.Price);

        //        return Ok(new
        //        {
        //            StatusCode = 200,
        //            Message = "Bookings retrieved successfully.",
        //            UserInfo = new
        //            {
        //                FullName = user.AspNetUser.UserName,
        //                Email = user.AspNetUser.Email,
        //                PassportNumber = user.PassportNumber
        //            },
        //            TotalRoomBookingsPrice = totalRoomsPrice,
        //            TotalActivityBookingsPrice = totalActivitiesPrice,
        //            TotalAllBookingsPrice = totalRoomsPrice + totalActivitiesPrice,
        //            Data = bookingList
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            StatusCode = 500,
        //            message = "An error occurred while retrieving bookings.",
        //            details = ex.Message
        //        });
        //    }
        //}

        [HttpGet("room-bookings/{userId}")]
        public async Task<ActionResult> GetRoomBookings(int userId)
        {
            try
            {
                var user = await dbContext.users
                    .Include(u => u.AspNetUser)
                    .FirstOrDefaultAsync(u => u.TouristId == userId);

                if (user == null)
                {
                    return NotFound(new { StatusCode = 404, message = "User not found." });
                }

                var roomBookings = await dbContext.bookings
                    .Where(b => b.TouristId == userId && b.RoomId != null)
                    .Include(b => b.Room)
                    .ToListAsync();

                if (!roomBookings.Any())
                {
                    return NotFound(new { StatusCode = 404, message = "No room bookings found." });
                }

                var result = roomBookings.Select(b => new
                {
                    BookingId = b.BookingId,
                    Status = b.Status.ToString(),
                    RoomId = b.Room.Id,
                    RoomName = b.Room.Name,
                    RoomPricePerNight = b.Room.PricePerNight,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    NumberOfNights = (b.EndDate - b.StartDate).Days,
                    NumberOfGuests = b.NumberOfGuests,
                    TotalPrice = b.Room.PricePerNight * (b.EndDate - b.StartDate).Days
                }).ToList();

                decimal total = result.Sum(r => (decimal)r.TotalPrice);

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Room bookings retrieved successfully.",
                    UserInfo = new
                    {
                        FullName = user.AspNetUser.UserName,
                        Email = user.AspNetUser.Email
                    },
                    TotalRoomBookingsPrice = total,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    message = "An error occurred while retrieving room bookings.",
                    details = ex.Message
                });
            }
        }

        #region EndPoint_CancelBooking

        [HttpDelete("cancel/{bookingId}")]
        public async Task<ActionResult> CancelBooking(int bookingId)
        {
            var booking = await dbContext.bookings
                .Include(b => b.Room)
                .Include(b => b.Payment)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound(new { StatusCode = 404, message = "Booking not available." });
            }

            // ✅ التأكد من عدم إلغاء حجز مدفوع مسبقًا
            if (booking.Payment != null && booking.Payment.Status == "Completed")
            {
                return BadRequest(new { StatusCode = 400, message = "Payment has already been completed, cannot cancel." });
            }

            // ✅ تحديث حالة الحجز إلى "Cancelled"
            booking.Status = Booking.BookingStatus.Cancelled;

            // ✅ إذا كان الدفع معلقًا، نقوم بتحديثه إلى "Cancelled"
            if (booking.Payment != null && booking.Payment.Status == "Pending")
            {
                booking.Payment.Status = "Cancelled";
            }

            // ✅ التحقق مما إذا كان يجب إعادة الغرفة إلى الحالة المتاحة
            if (booking.Room != null)
            {
                bool hasOtherBookings = await dbContext.bookings.AnyAsync(b =>
                    b.RoomId == booking.RoomId &&
                    b.Status != Booking.BookingStatus.Cancelled &&
                    ((b.StartDate >= booking.StartDate && b.StartDate < booking.EndDate) ||
                    (b.EndDate > booking.StartDate && b.EndDate <= booking.EndDate) ||
                    (b.StartDate <= booking.StartDate && b.EndDate >= booking.EndDate))
                );

                if (!hasOtherBookings)
                {
                    booking.Room.IsAvailable = true; // ✅ إعادة الغرفة للحالة المتاحة
                }
            }

            // ✅ حفظ التغييرات
            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Your booking has been successfully cancelled.",
                RoomId = booking.Room?.Id,
                IsAvailable = booking.Room?.IsAvailable
            });
        }

        #endregion





    }
}
