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
                
                var user = await _context.users
                    .Include(u => u.Bookings)
                        .ThenInclude(b => b.Room)
                            .ThenInclude(r => r.Hotel)
                                .ThenInclude(h => h.Location)
                    .FirstOrDefaultAsync(u => u.TouristId == userId);

                if (user == null)
                    return NotFound(new { StatusCode = 404, Message = "User not found." });

                var cartActivities = await _context.AddActivityToCarts
                    .Include(a => a.Activity)
                        .ThenInclude(ac => ac.locationActivities)
                    .Where(x => x.UserId == userId)
                    .ToListAsync();

 
                if (!cartActivities.Any())
                    return BadRequest(new { StatusCode = 400, Message = "No activities in the cart." });

                var confirmedRoomBooking = user.Bookings
                    .FirstOrDefault(b => b.RoomId != null && b.Status == Booking.BookingStatus.Confirmed);

                DateTime? startDate = null;
                DateTime? endDate = null;
                int? roomLocationId = null;

                if (confirmedRoomBooking != null)
                {
                    startDate = confirmedRoomBooking.StartDate;
                    endDate = confirmedRoomBooking.EndDate;
                    roomLocationId = confirmedRoomBooking.Room.Hotel.LocationId;

                    var mismatchedActivities = cartActivities
                        .Where(a => !a.Activity.locationActivities.Any(la => la.LocationId == roomLocationId))
                        .ToList();

                    if (mismatchedActivities.Any())
                        return BadRequest(new { StatusCode = 400, Message = "All selected activities must be in the same location as your booked room." });
                }
                else
                {
                    
                    startDate = DateTime.Now; 
                 
                }

                var hasPendingBooking = user.Bookings.Any(b => b.Status == Booking.BookingStatus.Pending);
                if (hasPendingBooking)
                {
                    return BadRequest(new { StatusCode = 400, Message = "You already have a pending booking. Please complete or cancel it before creating a new one." });
                } 

             
                var newBooking = new Booking
                {
                    TouristId = userId,
                    RoomId = null,
                    PaymentMethodId = 1,
                    StartDate = startDate ?? DateTime.MinValue, 
                    EndDate = endDate ?? DateTime.MinValue,
                    TotalPrice = cartActivities.Sum(a => a.NumberOfGuests * (decimal)a.Activity.Price),
                    NumberOfGuests = cartActivities.Sum(a => a.NumberOfGuests), 
                    Status = Booking.BookingStatus.Pending 
                };

             
                _context.bookings.Add(newBooking);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Activity booking prepared. Proceed to payment.",
                    BookingId = newBooking.BookingId,
                    HasRoom = confirmedRoomBooking != null 

                });
            }
            catch (Exception ex)
            {
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
              
                var user = await _context.users
                    .Include(u => u.Bookings)
                        .ThenInclude(b => b.Room)
                            .ThenInclude(r => r.Hotel)
                                .ThenInclude(h => h.Location)
                    .FirstOrDefaultAsync(u => u.TouristId == userId);

                if (user == null)
                    return NotFound(new { StatusCode = 404, Message = "User not found." });

                var cartActivities = await _context.AddActivityToCarts
                    .Include(a => a.Activity)
                        .ThenInclude(ac => ac.locationActivities)
                    .Where(x => x.UserId == userId)
                    .ToListAsync();

                if (!cartActivities.Any())
                    return BadRequest(new { StatusCode = 400, Message = "No activities in the cart." });

                  var confirmedRoomBooking = user.Bookings
                    .FirstOrDefault(b => b.RoomId != null && b.Status == Booking.BookingStatus.Confirmed);

                if (confirmedRoomBooking != null)
                {
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
                
                var user = await _context.users.FirstOrDefaultAsync(u => u.TouristId == userId);
                if (user == null)
                    return NotFound(new { StatusCode = 404, Message = "User not found." });

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


                existingBooking.Status = Booking.BookingStatus.Confirmed;
                existingBooking.StartDate = activityBookings.Min(ab => ab.StartDate);
                existingBooking.EndDate = activityBookings.Max(ab => ab.StartDate);
                existingBooking.TotalPrice = activityBookings.Sum(ab => ab.NumberOfGuests * (decimal)ab.ActivityPrice);
                existingBooking.NumberOfGuests = activityBookings.Sum(ab => ab.NumberOfGuests);
                existingBooking.PaymentTime = DateTime.UtcNow;

                var confirmedRoomBooking = await _context.bookings
                    .FirstOrDefaultAsync(b =>
                        b.TouristId == userId &&
                        b.Status == Booking.BookingStatus.Confirmed &&
                        b.RoomId != null
                    );

              
                foreach (var ab in activityBookings)
                {
                   
                    var cartItem = cartItems.FirstOrDefault(c => c.ActivityId == ab.ActivityId);
                    if (cartItem == null)
                        return BadRequest(new
                        {
                            StatusCode = 400,
                            Message = $"Activity with ID {ab.ActivityId} was not added to the cart."
                        });

                 
                    if (cartItem.ActivityPrice != ab.ActivityPrice)
                    {
                        return BadRequest(new
                        {
                            StatusCode = 400,
                            Message = $"Price for activity {ab.ActivityId} does not match the price in the cart."
                        });
                    }

                    if (cartItem.NumberOfGuests != ab.NumberOfGuests)
                    {
                        return BadRequest(new
                        {
                            StatusCode = 400,
                            Message = $"Number of guests for activity {ab.ActivityId} does not match the number in the cart."
                        });
                    }

                    if (cartItem.ActivityName.Trim().ToLower() != ab.ActivityName.Trim().ToLower())
                    {
                        return BadRequest(new
                        {
                            StatusCode = 400,
                            Message = $"Activity name for ID {ab.ActivityId} does not match the name in the cart. Expected: '{cartItem.ActivityName}', Provided: '{ab.ActivityName}'."
                        });
                    }

                    var requestedDate = ab.StartDate.Date;

                   
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

              
                    var bookingActivity = new BookingActivity
                    {
                        BookingId = existingBooking.BookingId,
                        ActivityId = ab.ActivityId,
                        ActivityDate = requestedDate,
                        NumberOfGuests = ab.NumberOfGuests
                    };
                    await _context.BookingActivities.AddAsync(bookingActivity);
                }

               
                var existingPayment = existingBooking.Payment;

               
                if (existingPayment == null)
                {
                    var newPayment = new Payment
                    {
                        BookingId = existingBooking.BookingId,
                        PaymentTime = DateTime.UtcNow,
                        Amount = existingBooking.TotalPrice,
                        PaymentMethodId = existingBooking.PaymentMethodId, 
                        Status = "Completed" 
                    };
                    await _context.Payments.AddAsync(newPayment);
                }
                else
                {
                    existingPayment.Status = "Completed";
                    existingPayment.PaymentTime = DateTime.UtcNow;
                    existingPayment.Amount = existingBooking.TotalPrice;
                    _context.Payments.Update(existingPayment);
                }

              
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
