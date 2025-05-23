using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism_project.Dtos.Home;
using Tourism_project.Models;
using Microsoft.Extensions.Logging;
using static Tourism_project.Models.Room;
using static Tourism_project.Models.Booking;

namespace Tourism_project.Controllers.Home
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;


        public RoomsController(ApplicationDbContext dbContext)
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
                _ => "Unknown" 
            };
        }

        #region EndPoint_BookRoom

        [HttpPost("book")]
        public async Task<ActionResult> BookRoom([FromBody] BookingDto request)
        {
            try
            {
               
                var userExists = await dbContext.users.AnyAsync(u => u.TouristId == request.UserId);
                if (!userExists)
                {
                    return BadRequest(new { StatusCode = 400, message = "User not found." });
                }

           
                var room = await dbContext.Rooms.FindAsync(request.RoomId);
                if (room == null)
                {
                    return BadRequest(new { StatusCode = 400, message = "The room does not exist." });
                }

           
                if (request.NumberOfGuests > room.MaxOccupancy)
                {
                    return BadRequest(new { StatusCode = 400, message = $"The selected room can only accommodate up to {room.MaxOccupancy} guests." });
                }

           
                if (request.PaymentMethodId <= 0)
                {
                    return BadRequest(new { StatusCode = 400, message = "Invalid Payment Method ID." });
                }

            
                var paymentMethod = await dbContext.paymentMethods.FindAsync(request.PaymentMethodId);
                if (paymentMethod == null)
                {
                    return BadRequest(new { StatusCode = 400, message = "Invalid Payment Method. This payment method does not exist." });
                }

                if (request.StartDate < DateTime.Today)
                {
                    return BadRequest(new { StatusCode = 400, message = "Start date cannot be in the past." });
                }

              
                int numberOfDays = (request.EndDate - request.StartDate).Days;
                if (numberOfDays <= 0)
                {
                    return BadRequest(new { StatusCode = 400, message = "End date must be after start date." });
                }


                
                bool isRoomBooked = await dbContext.bookings.AnyAsync(b =>
                    b.RoomId == request.RoomId &&
                    b.Status != Booking.BookingStatus.Cancelled &&
                    (
                        (request.StartDate >= b.StartDate && request.StartDate < b.EndDate) ||
                        (request.EndDate > b.StartDate && request.EndDate <= b.EndDate) ||
                        (request.StartDate <= b.StartDate && request.EndDate >= b.EndDate)
                    )
                );

                if (isRoomBooked)
                {
                    return BadRequest(new { StatusCode = 400, message = "The room is already booked for the selected dates." });
                }

              
                decimal totalPrice = room.PricePerNight * numberOfDays;

              
                var booking = new Booking
                {
                    RoomId = request.RoomId,
                    TouristId = request.UserId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    TotalPrice = totalPrice,
                    Status = Booking.BookingStatus.Pending,
                    PaymentMethodId = request.PaymentMethodId,
                    NumberOfGuests = request.NumberOfGuests
                };

                dbContext.bookings.Add(booking);
                await dbContext.SaveChangesAsync();

             
                DateTime transactionTime = DateTime.UtcNow;
                DateTime expirationTime = transactionTime.AddMinutes(5); 

                var payment = new Payment
                {
                    BookingId = booking.BookingId,
                    PaymentMethodId = request.PaymentMethodId,
                    Amount = booking.TotalPrice,
                    TransactionId = Guid.NewGuid().ToString(),
                    Status = "Pending",
                    PaymentTime = transactionTime
                };

                dbContext.Payments.Add(payment);
                await dbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Booking successful! Proceed to payment.",
                    bookingId = booking.BookingId,
                    transactionId = payment.TransactionId,
                    expiresAt = expirationTime,
                    expiresInMinutes = 5
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
                
                var payment = await dbContext.Payments
                    .Include(p => p.Booking) 
                    .FirstOrDefaultAsync(p => p.TransactionId == transactionId);

                if (payment == null)
                {
                    return NotFound(new { StatusCode = 404, message = "Payment transaction not found." });
                }

              
                if (payment.Status == "Completed")
                {
                    return BadRequest(new { StatusCode = 400, message = "Payment is already completed." });
                }

                
                if (payment.Booking == null)
                {
                    return BadRequest(new { StatusCode = 400, message = "Associated booking not found." });
                }

                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                       
                        payment.Status = "Completed";
                        payment.PaymentTime = DateTime.UtcNow; 
                        dbContext.Payments.Update(payment);

                       
                        payment.Booking.Status = Booking.BookingStatus.Confirmed;
                        payment.Booking.PaymentTime = DateTime.UtcNow;
                        dbContext.bookings.Update(payment.Booking);

                        await dbContext.SaveChangesAsync();
                        await transaction.CommitAsync(); 

                        return Ok(new
                        {
                            message = "Payment successful! Booking confirmed.",
                            bookingId = payment.BookingId,
                            paymentStatus = payment.Status
                        });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
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

            
            if (booking.Payment != null && booking.Payment.Status == "Completed")
            {
                return BadRequest(new { StatusCode = 400, message = "Payment has already been completed, cannot cancel." });
            }

           
            booking.Status = Booking.BookingStatus.Cancelled;

          
            if (booking.Payment != null && booking.Payment.Status == "Pending")
            {
                booking.Payment.Status = "Cancelled";
            }

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
                    booking.Room.IsAvailable = true; 
                }
            }

            
            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Your booking has been successfully cancelled.",
                RoomId = booking.Room?.Id,
                IsAvailable = booking.Room?.IsAvailable
            });
        }

        #endregion


        #region Endpoint_GetAllRooms

        [HttpGet("all-hotel-rooms")]
        public async Task<ActionResult> GetAllHotelRooms()
        {
            var rooms = await dbContext.Rooms
                .Select(r => new RoomDto
                {
                    RoomId = r.Id,
                    RoomName = r.Name,
                    MaxOccupancy = r.MaxOccupancy,
                    PricePerNight = r.PricePerNight,
                    BedCount = r.BedCount,
                    description = r.description,
                    IsAvailable = r.IsAvailable,
                    Size = r.Size,
                    RoomImageUrl = r.Media != null && r.Media.Any()
                        ? r.Media.OrderBy(m => m.MediaId).Select(m => m.MediaUrl).FirstOrDefault()
                        : "default-room-image.jpg"
                })
                .ToListAsync();

            return Ok(rooms);
        }
        #endregion

        #region Endpoint_GetAllRoomsByHotelID
        [HttpGet("hotel-rooms/{hotelId}")]
        public async Task<ActionResult> GetHotelRooms(int hotelId)
        {
            var rooms = await dbContext.Rooms
                .Where(r => r.HotelId == hotelId)
                .Select(r => new RoomDto
                {
                    RoomId = r.Id,
                    RoomName = r.Name,
                    MaxOccupancy = r.MaxOccupancy,
                    PricePerNight = r.PricePerNight,
                    BedCount = r.BedCount,
                    description = r.description,
                    IsAvailable = r.IsAvailable,
                    Size = r.Size,
                    RoomImageUrl = r.Media
                        .OrderBy(m => m.MediaId)
                        .FirstOrDefault().MediaUrl

                })
                .ToListAsync();

            return Ok(rooms);

        }
        #endregion

        #region Endpoint_GetRoomImagesById
        [HttpGet("room-images/{roomId}")]
        public async Task<IActionResult> GetRoomImages(int roomId)
        {
            
            if (roomId <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Invalid Room ID."
                });
            }

            
            bool roomExists = await dbContext.Rooms.AnyAsync(r => r.Id == roomId);
            if (!roomExists)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Room not found."
                });
            }

            
            var images = await dbContext.RoomMedias
                .Where(m => m.RoomId == roomId && m.MediaType == "image") 
                .OrderBy(m => m.MediaId) 
                .Select(m => m.MediaUrl) 
                .ToListAsync();

           
            if (!images.Any())
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No images found for this room."
                });
            }

            return Ok(new
            {
                StatusCode = 200,
                Message = "Room images retrieved successfully.",
                Images = images
            });
        }
        #endregion

        #region Endpoint_GetBookingDetailsRoomsbeforePayment 
        [HttpGet("booking-details-beforePayment/{bookingId}")]
        public async Task<ActionResult> GetBookingDetails(int bookingId)
        {
            try
            {
                
                var booking = await dbContext.bookings
                    .Include(b => b.Room) 
                    .Include(b => b.Payment)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    return NotFound(new { StatusCode = 404, message = "Booking not found." });
                }

                
                string bookingStatusName = Enum.GetName(typeof(BookingStatus), booking.Status);

               
                string roomTypeName = GetRoomTypeName((RoomType)booking.Room.Type);

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
                    paymentStatus = booking.Payment?.Status ?? "Pending", 
                    numberOfGuests = booking.NumberOfGuests,
                    bookingStatus = bookingStatusName
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
                
                var booking = await dbContext.bookings
                    .Include(b => b.Room)        
                    .Include(b => b.Payment)     
                    .Include(b => b.Tourist)
                    .ThenInclude(t => t.AspNetUser)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    return NotFound(new { StatusCode = 404, message = "Booking not found." });
                }

     
                if (booking.Status != Booking.BookingStatus.Confirmed)
                {
                    return BadRequest(new { StatusCode = 400, message = "Booking is not confirmed yet." });
                }

                
                string bookingStatusText = booking.Status.ToString();

              
                string roomTypeText = booking.Room.Type.ToString();

                var bookingDetails = new
                {
                    bookingId = booking.BookingId,

                    
                    user = new
                    {
                        fullName = booking.Tourist.AspNetUser.UserName,
                        passportNumber = booking.Tourist.PassportNumber,
                        email = booking.Tourist.AspNetUser.Email
                    },

                   
                    room = new
                    {
                        roomNumber = booking.Room.Id,
                        roomName = booking.Room.Name, 
                        roomType = roomTypeText,
                        pricePerNight = booking.Room.PricePerNight

                    },

                    
                    startDate = booking.StartDate,
                    endDate = booking.EndDate,
                    totalPrice = booking.TotalPrice,
                    paymentStatus = booking.Payment.Status,
                    paymentTime = booking.Payment.PaymentTime,
                    numberOfGuests = booking.NumberOfGuests,
                    bookingStatus = bookingStatusText 
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


        #region EndPoint_GetRoomDetails
        [HttpGet("{roomId}")]
        public async Task<ActionResult> GetRoomDetails(int roomId)
        {
            try
            {
                var room = await dbContext.Rooms
                     .Where(r => r.Id == roomId)
                     .Select(r => new
                     {
                         r.Id,
                         r.Name,
                         r.PricePerNight,
                         HotelName = r.Hotel.Name, 
                         HotelId = r.Hotel.HotelId, 
                         r.IsAvailable
                     })
                     .FirstOrDefaultAsync();

                if (room == null)
                {

                    return NotFound(new { StatusCode = 404, message = "Room not found." });
                }

                return Ok(room);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { StatusCode = 500, message = "Internal server error", details = ex.Message });
            }

        }


        #endregion

        

    }
}
