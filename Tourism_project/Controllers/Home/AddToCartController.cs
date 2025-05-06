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
                return BadRequest(new { Message = "Invalid request body.", StatusCode = 400 });
            }

            // التحقق من أن UserId و ActivityId غير فارغين
            if (addActivityToCartDTO.UserId <= 0 || addActivityToCartDTO.ActivityId <= 0)
            {
                return BadRequest(new { Message = "UserId and ActivityId must be valid.", StatusCode = 400 });
            }

            // التحقق من وجود النشاط بالفعل في السلة للمستخدم نفسه
            var existingCartItem = await _context.AddActivityToCarts
                .FirstOrDefaultAsync(x => x.UserId == addActivityToCartDTO.UserId && x.ActivityId == addActivityToCartDTO.ActivityId);

            if (existingCartItem != null)
            {
                return Conflict(new { Message = "Activity already added to cart.", StatusCode = 409 });
            }

            // جلب النشاط من قاعدة البيانات باستخدام ActivityId
            var activity = await _context.Activities
                .Include(a => a.locationActivities)
                .ThenInclude(la => la.Location)
                .FirstOrDefaultAsync(a => a.ActivityId == addActivityToCartDTO.ActivityId);

            // التحقق من أن النشاط موجود
            if (activity == null)
            {
                return NotFound(new { Message = "Activity not found.", StatusCode = 404 });
            }

            // التحقق من وجود مواقع مرتبطة بالنشاط
            var locationActivity = activity.locationActivities.FirstOrDefault();
            if (locationActivity == null || locationActivity.Location == null)
            {
                return NotFound(new { Message = "Location for the activity not found.", StatusCode = 404 });
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
                AddedAt = DateTime.Now
            };
            // إضافة النشاط إلى السلة
            _context.AddActivityToCarts.Add(addActivityToCart);
            await _context.SaveChangesAsync();

            // الاستجابة مع التفاصيل
            return Ok(new
            {
                Message = "Activity added to cart.",
                StatusCode = 200,
                AddedActivity = new
                {
                    addActivityToCart.Id,
                    addActivityToCart.UserId,
                    addActivityToCart.ActivityId,
                    addActivityToCart.ActivityName,
                    addActivityToCart.ActivityPrice,
                    addActivityToCart.ActivityImageUrl,
                    LocationName = locationName,
                    addActivityToCart.AddedAt
                }
            });
        }
        #endregion

        #region

        [HttpGet("cart/{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            var cartItems = await _context.AddActivityToCarts
                .Where(x => x.UserId == userId)
                .Include(x => x.Activity) // لعرض تفاصيل النشاط
                .ToListAsync();

            return Ok(cartItems);
        }

        #endregion



        #region

        [HttpDelete("remove-from-cart/{userId}/{activityId}")]
        public async Task<IActionResult> RemoveFromCart(int userId, int activityId)
        {
            var cartItem = await _context.AddActivityToCarts
                .FirstOrDefaultAsync(x => x.UserId == userId && x.ActivityId == activityId);

            if (cartItem == null)
            {
                return NotFound("Activity not found in cart.");
            }

            _context.AddActivityToCarts.Remove(cartItem);
            await _context.SaveChangesAsync();
            return Ok("Activity removed from cart.");
        }

        #endregion

        #region


        #endregion
    }
}
