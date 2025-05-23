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
    public class BookingController : ControllerBase
    {

        private readonly ApplicationDbContext dbContext;

        public BookingController (ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }




      

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





    }
}
