using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Tourism_project.Dtos.Home;
using Tourism_project.Models;

namespace Tourism_project.Controllers.Home
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivitysController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public ActivitysController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region EndPoint_GetAllActivities
        [HttpGet("api/activities")]
        public async Task<IActionResult> GetAllActivities()
        {
            try
            {

                var activities = await dbContext.Activities
                    .Select(a => new ActivityDto
                    {
                        Id = a.ActivityId,
                        Name = a.Name,
                        Description = a.Description,
                        Price = a.Price,
                        DurationHours = a.DurationHours, 
                        ImageUrl = a.ImageUrl,
                        LocationName = a.locationActivities.Select(al => al.Location.Name).FirstOrDefault(),
                    })
                    .ToListAsync();

                return Ok(activities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "An error occurred while fetching activities.", error = ex.Message });
            }
        }

        #endregion

        //#region EndPoint_AddActivity
        //[HttpPost("api/activities")]
        //public async Task<IActionResult> AddActivity([FromBody] ACtivity activityDto)
        //{
        //    try
        //    {
        //        if (activityDto == null)
        //        {
        //            return BadRequest(new { statusCode = 400, message = "Invalid data." });
        //        }

        //        var activity = new ACtivity
        //        {
        //            Name = activityDto.Name,
        //            Description = activityDto.Description,
        //            Price = activityDto.Price,
        //            DurationHours = activityDto.DurationHours
        //        };

        //        dbContext.Activities.Add(activity);
        //        await dbContext.SaveChangesAsync();

        //        return CreatedAtAction(nameof(GetActivityById), new { id = activity.ActivityId }, activity);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { statusCode = 500, message = "An error occurred while adding the activity.", error = ex.Message });
        //    }
        //}

        //#endregion

        #region EndPoint_GetActivityById
        [HttpGet("api/activities/{id}")]
        public async Task<IActionResult> GetActivityById(int id)
        {
            try
            {
                var activity = await dbContext.Activities
                    .Where(a => a.ActivityId == id)
                    .Select(a => new ActivityDto
                    {
                        Id = a.ActivityId,
                        Name = a.Name,
                        Description = a.Description,
                        Price = a.Price,
                        DurationHours = a.DurationHours,
                        ImageUrl = a.ImageUrl,
                        LocationName = a.locationActivities.Select(al => al.Location.Name).FirstOrDefault(),

                    })
                    .FirstOrDefaultAsync();

                if (activity == null)
                {
                    return NotFound(new { statusCode = 404, message = "Activity not found." });
                }

                return Ok(activity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "An error occurred while fetching the activity.", error = ex.Message });
            }
        }

        #endregion

        #region EndPoint_UpdateActivity
        [HttpPut("api/activities/{id}")]
        public async Task<IActionResult> UpdateActivity(int id, [FromBody] ACtivity activityDto)
        {
            try
            {
                if (activityDto == null || id != activityDto.ActivityId)
                {
                    return BadRequest(new { statusCode = 400, message = "Invalid data." });
                }

                var activity = await dbContext.Activities.FindAsync(id);

                if (activity == null)
                {
                    return NotFound(new { statusCode = 404, message = "Activity not found." });
                }
                activity.Name = activityDto.Name;
                activity.Description = activityDto.Description;
                activity.Price = activityDto.Price;
                activity.DurationHours = activityDto.DurationHours;

                dbContext.Activities.Update(activity);
                await dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "An error occurred while updating the activity.", error = ex.Message });
            }
        }

        #endregion

        #region EndPoint_DeleteActivity
        [HttpDelete("api/activities/{id}")]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            try
            {
                var activity = await dbContext.Activities.FindAsync(id);

                if (activity == null)
                {
                    return NotFound(new { statusCode = 404, message = "Activity not found." });
                }

                dbContext.Activities.Remove(activity);
                await dbContext.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "An error occurred while deleting the activity.", error = ex.Message });
            }
        }

        #endregion

        #region EndPoint_GetActivitiesByLocation
        // GET: api/activities/locations/5
        [HttpGet("locations/{locationId}")]
        public async Task<IActionResult> GetActivitiesByLocation(int locationId)
        {
            try
            {
                var activities = await dbContext.LocationActivities
                    .Where(la => la.LocationId == locationId)
                    .Select(la => la.Activity)
                    .Select(a => new ActivityDto
                    {
                        Id = a.ActivityId,
                        Name = a.Name,
                        Description = a.Description,
                        Price = a.Price,
                        ImageUrl = a.ImageUrl,
                        MoreDescription = a.MoreDescription,
                        DurationHours = a.DurationHours,
                        LocationName = a.locationActivities.Select(al => al.Location.Name).FirstOrDefault(),
                    })
                    .ToListAsync();

                if (activities.Count == 0)
                {
                    return NotFound(new { statusCode = 404, message = "No activities found for this location." });
                }

                return Ok(activities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "An error occurred while fetching activities for this location.", error = ex.Message });
            }
        }

        #endregion

        #region EndPoint_GetActivityDetails

        [HttpGet("Detials/{activityId}")]
        public async Task<IActionResult> GetActivityDetails(int  activityId)
        {
            try
            {
                var activity = await dbContext.Activities
                    .Where(a => a.ActivityId == activityId)
                    .Select(a => new
                    {
                        MoreDescription = a.MoreDescription,
                        ImageUrl = a.ImageUrl
                    })
                    .FirstOrDefaultAsync();

                if (activity == null)
                {
                    return NotFound(new { statusCode = 404, message = "Activity not found." });
                }
                return Ok(activity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { statusCode = 500, message = "An error occurred while fetching the activity details.", error = ex.Message });
            }
        }

        #endregion

    }
}
