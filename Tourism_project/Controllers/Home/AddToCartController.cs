using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism_project.Dtos.Home;
using Tourism_project.Models;

namespace Tourism_project.Controllers.Home
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddToCartController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AddToCartController(ApplicationDbContext dbContext)
        {
            this._context = dbContext;
        }

        #region

        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] AddActivityToCartDTO addActivityToCartDTO)
        {
            // التحقق من أن الـ DTO ليس فارغًا
            if (addActivityToCartDTO == null)
            {
                return BadRequest(new { StatusCode = 400, Message = "Invalid request body." });
            }

            // التحقق من أن UserId و ActivityId و NumberOfGuests غير فارغين
            if (addActivityToCartDTO.UserId <= 0 || addActivityToCartDTO.ActivityId <= 0 || addActivityToCartDTO.NumberOfGuests <= 0)
            {
                return BadRequest(new { StatusCode = 400, Message = "UserId, ActivityId, and NumberOfGuests must be valid." });
            }

            // التحقق من وجود المستخدم في قاعدة البيانات
            var userExists = await _context.users
                .AnyAsync(x => x.TouristId == addActivityToCartDTO.UserId);

            if (!userExists)
            {
                return NotFound(new { StatusCode = 404, Message = "User not found." });
            }

            // التحقق من وجود النشاط بالفعل في السلة للمستخدم نفسه
            var existingCartItem = await _context.AddActivityToCarts
                .FirstOrDefaultAsync(x => x.UserId == addActivityToCartDTO.UserId && x.ActivityId == addActivityToCartDTO.ActivityId);

            if (existingCartItem != null)
            {
                return Conflict(new { StatusCode = 409, Message = "Activity already added to cart." });
            }

            // جلب النشاط من قاعدة البيانات باستخدام ActivityId
            var activity = await _context.Activities
                .Include(a => a.locationActivities)
                .ThenInclude(la => la.Location)
                .FirstOrDefaultAsync(a => a.ActivityId == addActivityToCartDTO.ActivityId);

            // التحقق من أن النشاط موجود
            if (activity == null)
            {
                return NotFound(new { StatusCode = 404, Message = "Activity not found." });
            }

            // التحقق من وجود مواقع مرتبطة بالنشاط
            var locationActivity = activity.locationActivities.FirstOrDefault();
            if (locationActivity == null || locationActivity.Location == null)
            {
                return NotFound(new { StatusCode = 404, Message = "Location for the activity not found." });
            }

            string locationName = locationActivity.Location.Name;
            int locationId = locationActivity.Location.Id;
            var location = activity.locationActivities.FirstOrDefault()?.Location;

            // إنشاء كائن جديد من AddActivityToCart
            var addActivityToCart = new AddActivityToCart
            {
                UserId = addActivityToCartDTO.UserId,
                ActivityId = addActivityToCartDTO.ActivityId,
                ActivityName = activity.Name,
                ActivityPrice = (decimal)activity.Price,
                ActivityImageUrl = activity.ImageUrl,
                LocationId = location.Id,
                NumberOfGuests = addActivityToCartDTO.NumberOfGuests, // استخدام عدد الضيوف
                AddedAt = DateTime.Now
            };

            // إضافة النشاط إلى السلة
            _context.AddActivityToCarts.Add(addActivityToCart);
            await _context.SaveChangesAsync();

            // الاستجابة مع التفاصيل
            return Ok(new
            {
                StatusCode = 200,
                Message = "Activity added to cart.",
                AddedActivity = new
                {
                    addActivityToCart.Id,
                    addActivityToCart.UserId,
                    addActivityToCart.ActivityId,
                    addActivityToCart.ActivityName,
                    addActivityToCart.ActivityPrice,
                    addActivityToCart.ActivityImageUrl,
                    LocationName = locationName,
                    addActivityToCart.NumberOfGuests, // عرض عدد الضيوف
                    addActivityToCart.AddedAt
                }
            });
        }

        #endregion



        #region

        [HttpDelete("remove-from-cart/{userId}/{activityId}")]
        public async Task<IActionResult> RemoveFromCart(int userId, int activityId)
        {
            try
            {
                var userExists = await _context.users.AnyAsync(u => u.TouristId == userId);
                if (!userExists)
                {
                    return NotFound(new
                    {
                        StatusCode = 404,
                        Message = "User not found."
                    });
                }

                var cartItem = await _context.AddActivityToCarts
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.ActivityId == activityId);

                if (cartItem == null)
                {
                    return NotFound(new
                    {
                        StatusCode = 404,
                        Message = "Activity not found in cart."
                    });
                }

                _context.AddActivityToCarts.Remove(cartItem);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Activity removed from cart successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An error occurred while removing the activity from the cart.",
                    Details = ex.Message
                });
            }
        }



        #endregion

        #region

        [HttpGet("cart/{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            try
            {
                // تحقق من أن المستخدم موجود
                var userExists = await _context.users.AnyAsync(u => u.TouristId == userId);
                if (!userExists)
                {
                    return NotFound(new
                    {
                        StatusCode = 404,
                        Message = "User not found."
                    });
                }

                var cartItems = await _context.AddActivityToCarts
                    .Where(x => x.UserId == userId)
                    .Include(x => x.Activity)
                        .ThenInclude(a => a.locationActivities)
                            .ThenInclude(la => la.Location)
                    .Select(x => new
                    {
                        ActivityId = x.Activity.ActivityId,
                        ActivityName = x.Activity.Name,
                        Image = x.Activity.ImageUrl,
                        PricePerPerson = x.Activity.Price,
                        PeopleCount = x.NumberOfGuests, // ← تأكد من وجود هذا الحقل
                        TotalForThisActivity = x.NumberOfGuests * x.Activity.Price,
                        LocationNames = x.Activity.locationActivities
                            .Select(la => la.Location.Name)
                            .ToList()
                    })
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    return NotFound(new
                    {
                        StatusCode = 404,
                        Message = "Cart is empty.",
                        Data = cartItems
                    });
                }

                // حساب السعر الإجمالي لكل الأنشطة
                var totalCartPrice = cartItems.Sum(item => item.TotalForThisActivity);

                return Ok(new
                {
                    StatusCode = 200,
                    Message = "Cart retrieved successfully.",
                    TotalPrice = totalCartPrice,
                    Data = cartItems
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    StatusCode = 500,
                    Message = "An error occurred while retrieving the cart.",
                    Details = ex.Message
                });
            }
        }


        #endregion
    }
}
