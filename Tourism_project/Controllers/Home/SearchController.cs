using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism_project.Dtos.Home;
using Tourism_project.Models;

namespace Tourism_project.Controllers.Home
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public SearchController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region EndPoint_SearchTourismType
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TourismTypeDto>>> SearchTourismType([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { statusCode = 400, message = "Search query cannot be empty." });
            }

            var results = await dbContext.TourismTypes
                .Where(t => t.Name.Contains(query) || t.description.Contains(query))
                .Select(t => new TourismTypeDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    ImageUrl = t.ImageUrl,
                    Description = t.description,
                    is_active = t.is_active
                })
                .ToListAsync();

            if (results.Count == 0)
            {
                return NotFound(new { statusCode = 404, message = "No tourism types found matching the search criteria." });
            }

            return Ok(results);
        }
        #endregion

        #region EndPoint_SearchBy{Location , Hotal, Activity}
        // البحث عن الأماكن أو الفنادق أو الأنشطة حسب الكلمة المفتاحية
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { statusCode = 400, message = "Search query cannot be empty." });
            }

            var places = await dbContext.Locations
                .Where(p => p.Name.Contains(query) /* ||p.Description.Contains(query)*/ )
                .ToListAsync();

            var hotels = await dbContext.Hotels
                .Where(h => h.Name.Contains(query) || h.Description.Contains(query))
                .ToListAsync();

            var activities = await dbContext.Activities
                .Where(a => a.Name.Contains(query) || a.Description.Contains(query))
                .ToListAsync();

            return Ok(new { places, hotels, activities });
        }




        #endregion

        #region
        //// فلترة الأماكن حسب نوع السياحة ونطاق السعر
        //[HttpGet("filter")]
        //public async Task<IActionResult> FilterPlaces([FromQuery] int tourismTypeId, [FromQuery] string priceRange)
        //{
        //    var query = dbContext.Locations.AsQueryable();

        //    if (tourismTypeId > 0)
        //    {
        //        query = query.Where(p => p.Id == tourismTypeId);
        //    }

        //    if (!string.IsNullOrEmpty(priceRange) && priceRange.Contains("-"))
        //    {
        //        var prices = priceRange.Split('-');
        //        if (decimal.TryParse(prices[0], out decimal minPrice) && decimal.TryParse(prices[1], out decimal maxPrice))
        //        {
        //            query = query.Where(p => p.Price >= minPrice && p.Price <= maxPrice);
        //        }
        //    }

        //    var result = await query.ToListAsync();
        //    return Ok(result);
        //}
        #endregion

        #region


        #endregion

    }

}

