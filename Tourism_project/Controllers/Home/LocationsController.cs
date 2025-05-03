using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism_project.Dtos.Home;
using Tourism_project.Models;

namespace Tourism_project.Controllers.Home
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationsController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;


        public LocationsController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        #region EndPoint_GetLocationsByTourismType

        [HttpGet("GetLocationsByTourismType/{tourismTypeId}")]
        public async Task<IActionResult> GetLocationsByTourismType(int tourismTypeId)
        {
            try
            {
                bool tourismTypeExists = await dbContext.TourismTypes.AnyAsync(t => t.Id == tourismTypeId);
                if (!tourismTypeExists)
                {
                    return NotFound(new { statusCode = 404, message = "Tourism Type not found." });
                }

                var locations = await dbContext.TourismTypeLocations
                    .Where(ttl => ttl.TourismTypeId == tourismTypeId)
                    .Select(ttl => new LocationDto
                    {
                        Id = ttl.Location.Id,
                        Name = ttl.Location.Name,
                        ImageUrl = ttl.Location.ImageUrl,
                        Description= ttl.Location.description,
                        TourismType = new tourismTypeIDName
                        {
                            Id = ttl.TourismType.Id,
                            Name = ttl.TourismType.Name
                        }
                    })
                    .ToListAsync();

                if (!locations.Any())
                {
                    return NotFound(new { statusCode = 404, message = "No locations found for this tourism type." });
                }

                return Ok(locations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "An error occurred.", error = ex.Message });
            }
        }


        #endregion

        #region EndPoint_GetLocationById

        [HttpGet("GetLocationBy/{locationId}")]
        public async Task<IActionResult> GetLocationById(int locationId)
        {
            try
            {
                var location = await dbContext.TourismTypeLocations
                    .Where(ttl => ttl.LocationId == locationId)
                    .Select(ttl => new LocationDto
                    {
                        Id = ttl.Location.Id,
                        Name = ttl.Location.Name,
                        ImageUrl = ttl.Location.ImageUrl,
                        Description = ttl.Location.description,
                        TourismType = new tourismTypeIDName

                        {
                            Id = ttl.TourismType.Id,
                            Name = ttl.TourismType.Name
                        }
                    })
                    .FirstOrDefaultAsync();

                if (location == null)
                {
                    return NotFound(new { statusCode = 404, message = "Location not found." });
                }

                return Ok(location);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "An error occurred.", error = ex.Message });
            }
        }


        #endregion


        #region EndPoint_AddLocation
        [HttpPost("api/locations")]
        public async Task<IActionResult> AddLocation([FromBody] LocationDto locationDto)
        {
            try
            {
                if (locationDto == null)
                {
                    return BadRequest(new { statusCode = 400, message = "Invalid data." });
                }

                var location = new Location
                {
                    Name = locationDto.Name,
                    ImageUrl = locationDto.ImageUrl
                };

                dbContext.Locations.Add(location);
                await dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetLocationById), new { locationId = location.Id }, location);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "An error occurred while adding location.", error = ex.Message });
            }
        }

        #endregion



        #region EndPoint_UpdateLocation

        [HttpPut("api/locations/{id}")]
        public async Task<IActionResult> UpdateLocation(int id, [FromBody] LocationDto locationDto)
        {
            try
            {
                if (locationDto == null)
                {
                    return BadRequest(new { statusCode = 400, message = "Invalid data." });
                }

                var location = await dbContext.Locations.FindAsync(id);
                if (location == null)
                {
                    return NotFound(new { statusCode = 404, message = "Location not found." });
                }

                location.Name = locationDto.Name;
                location.ImageUrl = locationDto.ImageUrl;

                dbContext.Locations.Update(location);
                await dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "An error occurred while updating location.", error = ex.Message });
            }
        }

        #endregion

        #region EndPoint_DeleteLocation
        [HttpDelete("api/locations/{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                var location = await dbContext.Locations.FindAsync(id);
                if (location == null)
                {
                    return NotFound(new { statusCode = 404, message = "Location not found." });
                }

                dbContext.Locations.Remove(location);
                await dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "An error occurred while deleting location.", error = ex.Message });
            }
        }


        #endregion



    }
}
