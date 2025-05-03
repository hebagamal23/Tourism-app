//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Tourism_project.Dtos.Home;
//using Tourism_project.Interface;
//using Tourism_project.Models;

//namespace Tourism_project.Services
//{
//    public class FavoriteService : IFavoriteService
//    {
//        private readonly ApplicationDbContext dbContext;
//        private readonly ILogger<FavoriteService> _logger;

//        public FavoriteService(ApplicationDbContext dbContext, ILogger<FavoriteService> logger)
//        {
//            dbContext = dbContext;
//            _logger = logger;
//        }

//        public async Task<IActionResult> AddFavoriteAsync(AddFavoriteDto dto)
//        {


//            try
//            {
//                // 1. التحقق من وجود المستخدم
//                var user = await dbContext.users.FindAsync(dto.UserId);
//                if (user == null)
//                {
//                    var result = new  JsonResult(new
//                    {
//                        statusCode = 404,
//                        message = $"User with ID {dto.UserId} not found"
//                    });
//                    result.StatusCode = 404;  
//                    return result;
//                }

                
//                bool itemExists = false;

//                switch (dto.ItemType)
//                {
//                    case "Hotel":
//                        itemExists = await dbContext.Hotels.AnyAsync(h => h.HotelId == dto.ItemId);
//                        break;
//                    case "Activity":
//                        itemExists = await dbContext.Activities.AnyAsync(a => a.ActivityId == dto.ItemId);
//                        break;
//                    case "Location":
//                        itemExists = await dbContext.Locations.AnyAsync(l => l.Id == dto.ItemId);
//                        break;
//                    case "TourismType":
//                        itemExists = await dbContext.TourismTypes.AnyAsync(t => t.Id == dto.ItemId);
//                        break;
//                    default:
//                        var badRequestResult = new JsonResult(new
//                        {
//                            statusCode = 400,
//                            message = $"Invalid item type: {dto.ItemType}"
//                        });
//                        badRequestResult.StatusCode = 400;
//                        return badRequestResult;
//                }

//                if (!itemExists)
//                {
//                    var notFoundResult = new JsonResult(new
//                    {
//                        statusCode = 404,
//                        message = $"{dto.ItemType} with ID {dto.ItemId} not found"
//                    });
//                    notFoundResult.StatusCode=404; return notFoundResult;
//                }

//                // 3. التحقق من عدم التكرار
//                if (await dbContext.Favorites.AnyAsync(f =>
//                    f.UserId == dto.UserId &&
//                    f.ItemId == dto.ItemId &&
//                    f.ItemType == dto.ItemType))
//                {
//                    var conflictResult = new JsonResult(new
//                    {
//                        statusCode = 409,
//                        message = "Item already exists in favorites"
//                    });
//                    conflictResult.StatusCode = 409;
//                    return conflictResult;
//                }

//                // 4. إنشاء المفضلة
//                var favorite = new Favorite
//                {
//                    UserId = dto.UserId,
//                    ItemId = dto.ItemId,
//                    ItemType = dto.ItemType,
//                    AddedAt = DateTime.UtcNow
//                };

//                dbContext.Favorites.Add(favorite);
//                await dbContext.SaveChangesAsync();

//                var successResult = new JsonResult(new
//                {
//                    statusCode = 200,
//                    message = "Item added to favorites successfully"
//                });
//                successResult.StatusCode = 200;
//                return successResult;
//            }
//            catch (DbUpdateException dbEx)
//            {
//                var errorResult = new JsonResult(500, new
//                {
//                    statusCode = 500,
//                    message = "Database error: " + (dbEx.InnerException?.Message ?? dbEx.Message)
//                });
//                errorResult.StatusCode = 500;
//                return errorResult;
//            }
//            catch (Exception ex)
//            {
//                var errorResult = new JsonResult(500, new
//                {
//                    statusCode = 500,
//                    message = "Unexpected error: " + ex.Message
//                });
//                errorResult.StatusCode = 500;
//                return errorResult;
//            }
//        }

//        public async Task<IActionResult> RemoveFavoriteAsync(AddFavoriteDto dto)
//        {
//            // نفس المنطق داخل RemoveFromFavorites
//        }

//        public async Task<IActionResult> GetUserFavoritesAsync(int userId)
//        {
//            // نفس المنطق داخل GetUserFavorites
//        }
//    }
//}
