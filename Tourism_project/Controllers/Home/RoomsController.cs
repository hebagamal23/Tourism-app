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
            // ✅ التحقق من أن `roomId` صالح
            if (roomId <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Message = "Invalid Room ID."
                });
            }

            // ✅ التحقق مما إذا كانت الغرفة موجودة قبل البحث عن الصور
            bool roomExists = await dbContext.Rooms.AnyAsync(r => r.Id == roomId);
            if (!roomExists)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "Room not found."
                });
            }

            // ✅ جلب الصور الخاصة بالغرفة المطلوبة
            var images = await dbContext.RoomMedias
                .Where(m => m.RoomId == roomId && m.MediaType == "image") // جلب الصور فقط
                .OrderBy(m => m.MediaId) // ترتيب الصور حسب MediaId
                .Select(m => m.MediaUrl) // إرجاع الروابط فقط
                .ToListAsync();

            // ✅ التحقق مما إذا لم تكن هناك صور
            if (!images.Any())
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = "No images found for this room."
                });
            }

            // ✅ إرجاع الصور مع رسالة نجاح
            return Ok(new
            {
                StatusCode = 200,
                Message = "Room images retrieved successfully.",
                Images = images
            });
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
                         HotelName = r.Hotel.Name, // فقط تحميل اسم الفندق
                         HotelId = r.Hotel.HotelId, // يمكنك تحميل المزيد من البيانات حسب الحاجة
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
