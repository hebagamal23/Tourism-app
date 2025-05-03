using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism_project.Dtos.Home;
using Tourism_project.Models;

namespace Tourism_project.Controllers.Home
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavouriteController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ILogger<FavouriteController> _logger;

        public FavouriteController(ApplicationDbContext dbContext, ILogger<FavouriteController> logger)
        {
            this.dbContext = dbContext;
            _logger = logger;
        }

        #region EndPoint add-favorite  
        [HttpPost("add-favorite")]
        public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    statusCode = 400,
                    message = "Invalid input data"
                });
            }

            try
            {
                // 1. التحقق من وجود المستخدم
                var user = await dbContext.users.FindAsync(dto.UserId);
                if (user == null)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        message = $"User with ID {dto.UserId} not found"
                    });
                }

                // 2. التحقق من العنصر
                bool itemExists = false;

                switch (dto.ItemType)
                {
                    case "Hotel":
                        itemExists = await dbContext.Hotels.AnyAsync(h => h.HotelId == dto.ItemId);
                        break;
                    case "Activity":
                        itemExists = await dbContext.Activities.AnyAsync(a => a.ActivityId == dto.ItemId);
                        break;
                    case "Location":
                        itemExists = await dbContext.Locations.AnyAsync(l => l.Id == dto.ItemId);
                        break;
                    case "TourismType":
                        itemExists = await dbContext.TourismTypes.AnyAsync(t => t.Id == dto.ItemId);
                        break;
                    default:
                        return BadRequest(new
                        {
                            statusCode = 400,
                            message = $"Invalid item type: {dto.ItemType}"
                        });
                }

                if (!itemExists)
                {
                    return NotFound(new
                    {
                        statusCode = 404,
                        message = $"{dto.ItemType} with ID {dto.ItemId} not found"
                    });
                }

                // 3. التحقق من عدم التكرار
                if (await dbContext.Favorites.AnyAsync(f =>
                    f.UserId == dto.UserId &&
                    f.ItemId == dto.ItemId &&
                    f.ItemType == dto.ItemType))
                {
                    return Conflict(new
                    {
                        statusCode = 409,
                        message = "Item already exists in favorites"
                    });
                }

                // 4. إنشاء المفضلة
                var favorite = new Favorite
                {
                    UserId = dto.UserId,
                    ItemId = dto.ItemId,
                    ItemType = dto.ItemType,
                    AddedAt = DateTime.UtcNow
                };

                dbContext.Favorites.Add(favorite);
                await dbContext.SaveChangesAsync();

                return Ok(new
                {
                    statusCode = 200,
                    message = "Item added to favorites successfully"
                });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "Database error: " + (dbEx.InnerException?.Message ?? dbEx.Message)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    statusCode = 500,
                    message = "Unexpected error: " + ex.Message
                });
            }
        }
        #endregion

        #region EndPoint remove-favorite  
        [HttpDelete("remove-favorite")]
        public async Task<IActionResult> RemoveFromFavorites([FromBody] AddFavoriteDto request)
        {
            if (request == null || request.UserId <= 0 || request.ItemId <= 0 || string.IsNullOrEmpty(request.ItemType))
            {
                return BadRequest(new
                {
                    StatusCode = 400,
                    Error = "Invalid JSON request. Missing required fields."
                });
            }

            var favorite = await dbContext.Favorites
                .FirstOrDefaultAsync(f => f.UserId == request.UserId && f.ItemId == request.ItemId && f.ItemType == request.ItemType);

            if (favorite == null)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Error = "Item not found in favorites."
                });
            }

            dbContext.Favorites.Remove(favorite);
            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Item removed from favorites successfully."
            });
        }
        #endregion

        #region EndPoint user-favorites 
        [HttpGet("user-favorites/{userId}")]
        public async Task<IActionResult> GetUserFavorites(int userId)
        {
            // ✅ التحقق مما إذا كان المستخدم موجودًا
            bool userExists = await dbContext.users.AnyAsync(u => u.TouristId == userId);
            if (!userExists)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Error = "User not found."
                });
            }

            // ✅ جلب المفضلات الخاصة بالمستخدم
            var favorites = await dbContext.Favorites
                .Where(f => f.UserId == userId)
                .ToListAsync();

            if (!favorites.Any())
            {
                return Ok(new
                {
                    StatusCode = 200,
                    Message = "No favorite items found.",
                    Data = new List<object>()
                });
            }

            // ✅ تصنيف المفضلات حسب النوع
            var hotelIds = favorites.Where(f => f.ItemType == "Hotel").Select(f => f.ItemId).ToList();
            var activityIds = favorites.Where(f => f.ItemType == "Activity").Select(f => f.ItemId).ToList();
            var locationIds = favorites.Where(f => f.ItemType == "Location").Select(f => f.ItemId).ToList();
            var tourismTypeIds = favorites.Where(f => f.ItemType == "TourismType").Select(f => f.ItemId).ToList();

            // ✅ جلب تفاصيل العناصر المطلوبة لكل نوع
            var hotels = await dbContext.Hotels
                .Where(h => hotelIds.Contains(h.HotelId))
                .ToDictionaryAsync(h => h.HotelId);

            var activities = await dbContext.Activities
                .Where(a => activityIds.Contains(a.ActivityId))
                .ToDictionaryAsync(a => a.ActivityId);

            var locations = await dbContext.Locations
                .Where(l => locationIds.Contains(l.Id))
                .ToDictionaryAsync(l => l.Id);

            var tourismTypes = await dbContext.TourismTypes
                .Where(t => tourismTypeIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id);

            // ✅ جلب الصور المرتبطة بالفنادق
            var hotelImages = await dbContext.Media
                .Where(m => hotelIds.Contains(m.HotelId))
                .GroupBy(m => m.HotelId)
                .Select(g => new
                {
                    HotelId = g.Key,
                    ImageUrl = g.FirstOrDefault().MediaUrl // خذ أول صورة فقط لكل فندق
                })
                .ToDictionaryAsync(g => g.HotelId);

            // ✅ تشكيل قائمة المفضلات
            var favoriteItems = favorites.Select(f => new
            {
                f.FavoriteId,
                f.ItemId,
                f.ItemType,
                IsFavorite = true, // ثابت
                f.AddedAt,
                ItemDetails = f.ItemType switch
                {
                    "Hotel" => hotels.TryGetValue(f.ItemId, out var hotel) ? new
                    {
                        hotel.HotelId,
                        hotel.Name,
                        hotel.Address,
                        hotel.PricePerNight,
                        Image = hotelImages.TryGetValue(hotel.HotelId, out var img) ? img.ImageUrl : null


                    } as object : null,

                    "Activity" => activities.TryGetValue(f.ItemId, out var activity) ? new
                    {
                        activity.ActivityId,
                        activity.Name,
                        activity.Price,
                        activity.ImageUrl
                    } as object : null,

                    "Location" => locations.TryGetValue(f.ItemId, out var location) ? new
                    {
                        location.Id,
                        location.Name,
                        location.description,
                        location.ImageUrl
                    } as object : null,

                    "TourismType" => tourismTypes.TryGetValue(f.ItemId, out var tourismType) ? new
                    {
                        tourismType.Id,
                        tourismType.Name,
                        tourismType.description,
                        tourismType.ImageUrl
                    } as object : null,

                    _ => null
                }
            });

            return Ok(new
            {
                StatusCode = 200,
                Message = "User favorites retrieved successfully.",
                Data = favoriteItems
            });
        }
        #endregion
    }
}
