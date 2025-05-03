using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism_project.Dtos.Home;
using Tourism_project.Models;

namespace Tourism_project.Controllers.Home
{
    [Route("api/[controller]")]
    [ApiController]
    public class TourismTypeController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public TourismTypeController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region EndPoint_GetTourismTypes
        [HttpGet("GetTourismTypes")]
        public async Task<IActionResult> GetTourismTypes()
        {
            var types = await dbContext.TourismTypes
                .Select(t => new TourismTypeDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    ImageUrl = t.ImageUrl,
                    Description = t.description,
                    is_active = t.is_active
                })
                .ToListAsync();

            if (types == null || !types.Any())
            {
                return NotFound(new { statusCode = 404, message = "No tourism types found." });
            }

            return Ok(types);
        }
        #endregion

        #region EndPoint_GetTourismType
        [HttpGet]
        public async Task<ActionResult> GetTourismTypeById([FromQuery] int id)
        {
            var tourismType = await dbContext.TourismTypes
                .Where(t => t.Id == id)
                .Select(t => new TourismTypeDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    ImageUrl = t.ImageUrl,
                    Description = t.description,
                    is_active = t.is_active
                })
                .FirstOrDefaultAsync();

            if (tourismType == null)
            {
                return NotFound(new { statusCode = 404, message = $"Tourism type with id {id} not found." });
            }

            return Ok(tourismType);
        }
        #endregion

        #region EndPoint_AddTourismType
        [HttpPost]
        public async Task<ActionResult> PostTourismType(TourismTypeDto tourismTypeDto)
        {
            if (tourismTypeDto == null)
            {
                return BadRequest(new { statusCode = 400, message = "Invalid tourism type data." });
            }

            var tourismType = new TourismType
            {
                Name = tourismTypeDto.Name,
                description = tourismTypeDto.Description,
                ImageUrl = tourismTypeDto.ImageUrl,
                is_active = tourismTypeDto.is_active
            };

            var existingTourismType = await dbContext.TourismTypes
                .FirstOrDefaultAsync(t => t.Name == tourismType.Name);

            if (existingTourismType != null)
            {
                return Conflict(new { statusCode = 409, message = $"Tourism type with name {tourismType.Name} already exists." });
            }

            dbContext.TourismTypes.Add(tourismType);
            await dbContext.SaveChangesAsync();

            var createdTourismType = new TourismTypeDto
            {
                Id = tourismType.Id,
                Name = tourismType.Name,
                ImageUrl = tourismType.ImageUrl,
                Description = tourismType.description,
                is_active = tourismType.is_active
            };

            return CreatedAtAction(nameof(GetTourismTypeById), new { id = tourismType.Id }, createdTourismType);
        }
        #endregion

        #region EndPoint_PutTourismType
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTourismType(int id, TourismTypeDto tourismTypeDto)
        {
            if (id != tourismTypeDto.Id)
            {
                return BadRequest(new { statusCode = 400, message = "ID mismatch between URL and body data." });
            }

            var existingTourismType = await dbContext.TourismTypes.FindAsync(id);
            if (existingTourismType == null)
            {
                return NotFound(new { statusCode = 404, message = $"Tourism type with id {id} not found." });
            }

            existingTourismType.Name = tourismTypeDto.Name;
            existingTourismType.ImageUrl = tourismTypeDto.ImageUrl;
            existingTourismType.description = tourismTypeDto.Description;
            existingTourismType.is_active = tourismTypeDto.is_active;

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, new { statusCode = 500, message = "Internal server error during update operation." });
            }

            var updatedTourismTypeDto = new TourismTypeDto
            {
                Id = existingTourismType.Id,
                Name = existingTourismType.Name,
                ImageUrl = existingTourismType.ImageUrl,
                Description = existingTourismType.description,
                is_active = existingTourismType.is_active
            };

            return Ok(updatedTourismTypeDto);
        }
        #endregion

        #region EndPoint_DeleteTourismType
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTourismType(int id)
        {
            var tourismType = await dbContext.TourismTypes.FindAsync(id);
            if (tourismType == null)
            {
                return NotFound(new { statusCode = 404, message = $"Tourism type with id {id} not found." });
            }

            dbContext.TourismTypes.Remove(tourismType);
            await dbContext.SaveChangesAsync();

            return Ok(new { statusCode = 200, message = $"Tourism type with id {id} has been deleted successfully." });
        }
        #endregion

        #region EndPoint_SearchTourismTypeByNameAndDescribtion
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


        #region EndPoint_{id}/image-discount
        [HttpGet("image-discount")]
        public async Task<ActionResult> GetTourismTypeImageWithDiscount([FromQuery] int id)
        {
            var tourismType = await dbContext.TourismTypes
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    ImageUrl = t.ImageUrl
                })
                .FirstOrDefaultAsync();

            if (tourismType == null || string.IsNullOrEmpty(tourismType.ImageUrl))
            {
                return NotFound(new { statusCode = 404, message = $"Image for tourism type with id {id} not found." });
            }

            // **Generate a dynamic discount percentage**
            var random = new Random();
            int discountPercentage = random.Next(10, 51); // Generates a random number between 10% and 50%

            return Ok(new
            {
                ImageUrl = tourismType.ImageUrl,
                Message = $"Enjoy a {discountPercentage}% discount on this tourism package!"
            });
        }

        #endregion
        private bool TourismTypeExists(int id)
        {
            return dbContext.TourismTypes.Any(e => e.Id == id);
        }



    }
}
