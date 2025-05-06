using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism_project.Dtos.Home;
using Tourism_project.Models;

namespace Tourism_project.Controllers.Home
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {

        private readonly ApplicationDbContext dbContext;

        public BookingsController (ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        #region EndPointGetAllBooking 
        // Endpoint لاسترجاع الحجز مع الحقول المحددة
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookings()
        {
            // استرجاع الحجوزات مع الحقول المطلوبة فقط
            var bookings = await dbContext.bookings
                .Include(b => b.Tourist)  // الانضمام إلى السياح للحصول على اسمهم
                .Select(b => new BookingDtoOutput
                {
                    BookingId = b.BookingId,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    NumberOfGuests = b.NumberOfGuests,
                    TouristName = b.Tourist.AspNetUser.UserName,  // افترض أن لديك خاصية Name في كائن Tourist
                    BookingStatus = b.Status.ToString() // إضافة حالة الحجز
                })
                .ToListAsync();

            if (bookings == null || !bookings.Any())
            {
                return NotFound(new { message = "Not Found Booking Now ." });
            }

            return Ok(bookings);
        }

        #endregion



    }
}
