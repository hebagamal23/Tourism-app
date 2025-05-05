using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Tourism_project.Dtos.Home;
using Tourism_project.Models;

namespace Tourism_project.Controllers.Home
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public HotelsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region Endpoint لاسترجاع الفنادق الخاصة بموقع معين عن طريق الـ ID 
        [HttpGet("by-location/{locationId:int}")]
        public async Task<IActionResult> GetHotelsByLocationId(int locationId)
        {
            try
            {
                // البحث عن الفنادق المرتبطة بالموقع
                var hotels = await dbContext.Hotels
                    .Where(h => h.LocationId == locationId)
                    .Select(h => new HotelDto
                    {
                        HotelId = h.HotelId,
                        PricePerNight = h.PricePerNight,
                        Name = h.Name,
                        Address = h.Address,
                        Stars = h.Stars,
                        FirstImageUrl = h.Media != null && h.Media.Any()
                                        ? h.Media.OrderBy(m => m.MediaId).Select(m => m.MediaUrl).FirstOrDefault()
                                        : "default-image-url.jpg"
                    })
                    .ToListAsync();

                // إذا لم يتم العثور على فنادق
                if (!hotels.Any())
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        message = "No hotels found for the given location ID."
                    });
                }

                return Ok(new
                {
                    statusCode = 200,
                    message = "Hotels retrieved successfully.",
                    data = hotels
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "An error occurred while retrieving hotels.",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region Endpoint لاسترجاع الفنادق الخاصة بموقع معين عن طريق الاسم
        [HttpGet("by-location-name/{locationName}")]
        public async Task<IActionResult> GetHotelsByLocationName(string locationName)
        {
            // التحقق من صحة إدخال locationName
            if (string.IsNullOrWhiteSpace(locationName))
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Location name cannot be empty."
                });
            }

            try
            {
                // البحث عن الفنادق باستخدام البحث الجزئي مع Contains()
                locationName = locationName.ToLower(); // تحسين الأداء بتجنب التكرار
                var hotels = await dbContext.Hotels
                    .Where(h => h.Location.Name.ToLower().Contains(locationName))
                    .Select(h => new HotelDto
                    {
                        HotelId = h.HotelId,
                        Name = h.Name,
                        LocationName = h.Location.Name,
                        Address = h.Address,
                        PricePerNight = h.PricePerNight,
                        Stars = h.Stars,
                        FirstImageUrl = h.Media != null && h.Media.Any()
                                        ? h.Media.OrderBy(m => m.MediaId).Select(m => m.MediaUrl).FirstOrDefault()
                                        : "default-image-url.jpg"
                    })
                    .ToListAsync();

                // إذا لم يتم العثور على أي فنادق
                if (!hotels.Any())
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        message = "No hotels found for the given location name."
                    });
                }

                return Ok(new
                {
                    statusCode = 200,
                    message = "Hotels retrieved successfully.",
                    data = hotels
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "An error occurred while retrieving hotels.",
                    error = ex.Message
                });
            }
        }
        #endregion


        #region Endpoint لاسترجاع كل الصور الخاصة بفندق معين بناءً على HotelId

        [HttpGet("hotel/{hotelId}/images")]
        public async Task<IActionResult> GetImagesByHotelId(int hotelId)
        {
            // التحقق من صحة hotelId
            if (hotelId <= 0)
            {
                return BadRequest(new { statusCode = 400, message = "Invalid hotel ID." });
            }

            // البحث عن الصور الخاصة بالفندق باستخدام HotelId
            var images = await dbContext.Media
                .Where(m => m.HotelId == hotelId)
                .Select(m => new
                {
                    m.MediaId,
                    m.MediaUrl
                })
                .ToListAsync();

            // إذا لم يتم العثور على أي صور للفندق
            if (!images.Any())
            {
                return NotFound(new { statusCode = 404, message = "No images found for this hotel." });
            }

            // إرجاع الصور الخاصة بالفندق
            return Ok(images);
        }
        #endregion


        #region EndPoint_GethotelsByhotelId
        [HttpGet("GetHotelBy/{hotelId}")]
        public async Task<IActionResult> GetHotelById(int hotelId)
        {
            try
            {
                // التحقق من وجود الفندق
                var hotel = await dbContext.Hotels
                    .Where(h => h.HotelId == hotelId)
                    .FirstOrDefaultAsync();

                // إذا لم يتم العثور على الفندق
                if (hotel == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        message = "Hotel not found."
                    });
                }

                // إرجاع تفاصيل الفندق
                return Ok(new
                {
                    Id = hotel.HotelId,
                    Name = hotel.Name,
                    Rating = hotel.Rating,
                    priceprenight=hotel.PricePerNight,
                    Description = hotel.Description,
                    EstablishedDate = hotel.EstablishedDate,
                    MaxRooms = hotel.MaxRooms,
                    DistanceFromLocation = hotel.DistanceFromLocation,
                    
                    LocationId = hotel.LocationId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "An error occurred while retrieving the hotel.",
                    error = ex.Message
                });
            }
        }

        #endregion


        #region EndPoint_GetRating

        [HttpGet("ratings")]
        public async Task<IActionResult> GetHotelsByRating(int minRating)
        {
            try
            {
                // التحقق من أن التقييم صحيح بين 1 و 10
                if (minRating < 1 || minRating > 10)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        message = "Invalid rating. Rating should be between 1 and 10."
                    });
                }

                // البحث عن الفنادق التي تتوافق مع التقييم المحدد وإرجاعها كـ DTO
                var ratedHotels = await dbContext.Hotels
                    .Where(h => h.Rating >= minRating)
                    .Select(h => new HotelDto
                    {
                        HotelId = h.HotelId,
                        Name = h.Name,
                        Address = h.Address,
                        Stars = h.Stars,
                        PricePerNight = h.PricePerNight,
                       FirstImageUrl = h.Media != null && h.Media.Any()
                                        ? h.Media.OrderBy(m => m.MediaId)
                                        .Select(m => m.MediaUrl).FirstOrDefault()
                                        : "default-image-url.jpg"
                    })
                    .ToListAsync();

                // إذا لم توجد فنادق بالتقييم المطلوب
                if (!ratedHotels.Any())
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        message = "No hotels found with the specified rating."
                    });
                }

                // إرجاع قائمة الفنادق المتاحة في DTO
                return Ok(new
                {
                    statusCode = 200,
                    message = "Hotels retrieved successfully.",
                    data = ratedHotels
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "An error occurred while retrieving hotels by rating.",
                    error = ex.Message
                });
            }
        }

        #endregion

         
        #region EndPoint_GetHotelsByStars

        [HttpGet("hotels-by-stars")]
        public async Task<IActionResult> GetHotelsByStars(int minStars)
        {
            try
            {
                // التحقق من أن عدد النجوم صحيح بين 1 و 5
                if (minStars < 1 || minStars > 5)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        message = "Invalid star rating. Stars should be between 1 and 5."
                    });
                }

                // البحث عن الفنادق التي تتوافق مع عدد النجوم المحدد وإرجاعها كـ DTO
                var starRatedHotels = await dbContext.Hotels
                    .Where(h => h.Stars == minStars)
                    .Select(h => new HotelDto
                    {
                        HotelId = h.HotelId,
                        Name = h.Name,
                        Address = h.Address,
                        PricePerNight = h.PricePerNight,
                        Stars = h.Stars,
                        FirstImageUrl = h.Media != null && h.Media.Any()
                                        ? h.Media.OrderBy(m => m.MediaId)
                                        .Select(m => m.MediaUrl).FirstOrDefault()
                                        : "default-image-url.jpg"
                    })
                    .ToListAsync();

                // إذا لم توجد فنادق بعدد النجوم المطلوب
                if (!starRatedHotels.Any())
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        message = "No hotels found with the specified star rating."
                    });
                }

                // إرجاع قائمة الفنادق المتاحة في DTO
                return Ok(new
                {
                    statusCode = 200,
                    message = "Hotels retrieved successfully.",
                    data = starRatedHotels
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "An error occurred while retrieving hotels by stars.",
                    error = ex.Message
                });
            }
        }

        #endregion

        #region EndPoint_GetHotelsByPrice

        [HttpGet("hotels-by-price")]
        public async Task<IActionResult> GetHotelsByPrice(decimal minPrice, decimal maxPrice)
        {
            try
            {
                // ✅ التحقق من صحة القيم المدخلة
                if (minPrice < 0 || maxPrice <= 0 || minPrice > maxPrice)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        message = "Invalid price range. Ensure that minPrice is >= 0 and maxPrice is greater than minPrice."
                    });
                }

                // ✅ البحث عن الفنادق التي يتراوح سعرها بين الحد الأدنى والأقصى
                var hotelsInRange = await dbContext.Hotels
                    .Where(h => h.PricePerNight >= minPrice && h.PricePerNight <= maxPrice)
                    .Select(h => new HotelDto
                    {
                        HotelId = h.HotelId,
                        Name = h.Name,
                        Address = h.Address,
                        PricePerNight = h.PricePerNight,
                        FirstImageUrl = h.Media != null && h.Media.Any()
                                        ? h.Media.OrderBy(m => m.MediaId).Select(m => m.MediaUrl).FirstOrDefault()
                                        : "default-image-url.jpg"
                    })
                    .ToListAsync();

                // ✅ التحقق مما إذا لم يتم العثور على أي فنادق ضمن النطاق المطلوب
                if (!hotelsInRange.Any())
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        message = "No hotels found within the specified price range."
                    });
                }

                // ✅ إرجاع قائمة الفنادق المتاحة ضمن النطاق السعري
                return Ok(new
                {
                    statusCode = 200,
                    message = "Hotels retrieved successfully.",
                    data = hotelsInRange
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "An error occurred while retrieving hotels by price.",
                    error = ex.Message
                });
            }
        }

        #endregion


        #region EndPoint_GetHotelsByDateRange
        [HttpGet("available-hotels")]
        public async Task<IActionResult> GetAvailableHotels(DateTime startDate, DateTime endDate, int? guests = null, int? roomsRequired = null)
        {
            try
            {
                // ✅ التحقق من صحة التواريخ
                if (startDate >= endDate)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        message = "Invalid date range. Start date must be before end date."
                    });
                }

                // ✅ التحقق من أن تاريخ البداية ليس في الماضي
                if (startDate < DateTime.Today)
                {
                    return BadRequest(new
                    {
                        statusCode = 400,
                        message = "Start date cannot be in the past."
                    });
                }

                // البحث عن الفنادق المتاحة
                var availableHotels = await dbContext.Hotels
                    .Where(h => h.Rooms.Any(r =>
                        !r.Bookings.Any(b =>
                            (startDate < b.EndDate && endDate > b.StartDate) // تعارض الحجز
                        ) &&
                        (guests == null || r.MaxOccupancy >= guests) &&
                        (roomsRequired == null || h.Rooms.Count(r =>
                            !r.Bookings.Any(b =>
                                (startDate < b.EndDate && endDate > b.StartDate)
                            )
                        ) >= roomsRequired)
                    ))
                    .Select(h => new
                    {
                        HotelId = h.HotelId,
                        Name = h.Name,
                        Address = h.Address,
                        Stars = h.Stars,
                        priceprenight = h.PricePerNight,
                        AvailableRooms = h.Rooms.Count(r =>
                            !r.Bookings.Any(b =>
                                (startDate < b.EndDate && endDate > b.StartDate)
                            )
                        ),
                        FirstImageUrl = h.Media != null && h.Media.Any()
                            ? h.Media.OrderBy(m => m.MediaId).Select(m => m.MediaUrl).FirstOrDefault()
                            : "default-image-url.jpg"
                    })
                    .ToListAsync();

                // ✅ التحقق مما إذا لم يتم العثور على فنادق متاحة
                if (!availableHotels.Any())
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        message = "No hotels available for the selected period."
                    });
                }

                // ✅ إرجاع قائمة الفنادق المتاحة
                return Ok(new
                {
                    statusCode = 200,
                    message = "Available hotels retrieved successfully.",
                    data = availableHotels
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "An error occurred while retrieving available hotels.",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region EndPoint_GetAllHotels
        [HttpGet("GetAllHotels")]
        public async Task<IActionResult> GetAllHotels()
        {
            try
            {
                // ✅ جلب جميع الفنادق من قاعدة البيانات
                var hotels = await dbContext.Hotels
                    .Select(h => new
                    {
                        Id = h.HotelId,
                        Name = h.Name,
                        Rating = h.Rating,
                        PricePerNight = h.PricePerNight,
                        Description = h.Description,
                        EstablishedDate = h.EstablishedDate,
                        MaxRooms = h.MaxRooms,
                        DistanceFromLocation = h.DistanceFromLocation,
                        LocationId = h.LocationId,
                        FirstImageUrl = h.Media != null && h.Media.Any()
                            ? h.Media.OrderBy(m => m.MediaId).Select(m => m.MediaUrl).FirstOrDefault()
                            : "default-image-url.jpg"
                    })
                    .ToListAsync();

                // ✅ التحقق مما إذا لم يتم العثور على أي فنادق
                if (!hotels.Any())
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        message = "No hotels found."
                    });
                }

                // ✅ إرجاع قائمة الفنادق
                return Ok(new
                {
                    statusCode = 200,
                    message = "Hotels retrieved successfully.",
                    data = hotels
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "An error occurred while retrieving the hotels.",
                    error = ex.Message
                });
            }
        }
        #endregion
 

        [HttpGet("filter-hotels")]
        public async Task<IActionResult> FilterHotels(
    DateTime startDate,
    DateTime endDate,
    int? minStars = null,
    decimal? minPrice = null,
    decimal? maxPrice = null,
    int? guests = null,
    int? roomsRequired = null)
        {
            try
            {
                // ✅ التحقق من صحة التواريخ (إجباري)
                if (startDate >= endDate)
                {
                    return BadRequest(new { statusCode = 400, message = "Invalid date range. Start date must be before end date." });
                }

                if (startDate < DateTime.Today)
                {
                    return BadRequest(new { statusCode = 400, message = "Start date cannot be in the past." });
                }

                // ✅ بناء الاستعلام الأساسي
                IQueryable<Hotel> query = dbContext.Hotels;

                // ✅ تصفية حسب عدد النجوم (اختياري)
                if (minStars.HasValue)
                {
                    query = query.Where(h => h.Stars == minStars);
                }

                // ✅ تصفية حسب السعر (اختياري)
                if (minPrice.HasValue)
                {
                    query = query.Where(h => h.PricePerNight >= minPrice);
                }
                if (maxPrice.HasValue)
                {
                    query = query.Where(h => h.PricePerNight <= maxPrice);
                }

                // ✅ تصفية حسب التواريخ (إجباري)
                query = query.Where(h => h.Rooms.Any(r =>
                    !r.Bookings.Any(b =>
                        (startDate < b.EndDate && endDate > b.StartDate) // الحجز لا يتعارض مع التواريخ المحددة
                    ) &&
                    (guests == null || r.MaxOccupancy >= guests) &&
                    (roomsRequired == null || h.Rooms.Count(r =>
                        !r.Bookings.Any(b =>
                            (startDate < b.EndDate && endDate > b.StartDate)
                        )
                    ) >= roomsRequired)
                ));

                // ✅ تنفيذ الاستعلام وتحويل النتيجة إلى DTO
                var filteredHotels = await query.Select(h => new HotelDto
                {
                    HotelId = h.HotelId,
                    Name = h.Name,
                    Address = h.Address,
                    PricePerNight = h.PricePerNight,
                    Stars = h.Stars,
                    FirstImageUrl = h.Media != null && h.Media.Any()
                        ? h.Media.OrderBy(m => m.MediaId).Select(m => m.MediaUrl).FirstOrDefault()
                        : "default-image-url.jpg"
                }).ToListAsync();

                // ✅ التحقق مما إذا لم يتم العثور على أي فنادق
                if (!filteredHotels.Any())
                {
                    return NotFound(new { statusCode = 404, message = "No hotels found matching the given criteria." });
                }

                // ✅ إرجاع القائمة النهائية للفنادق
                return Ok(new { statusCode = 200, message = "Hotels retrieved successfully.", data = filteredHotels });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "An error occurred while filtering hotels.",
                    error = ex.Message
                });
            }
        }
 

    }
}
