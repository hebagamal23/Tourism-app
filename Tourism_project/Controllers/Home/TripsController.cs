//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Tourism_project.Models;

//namespace Tourism_project.Controllers.Home
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class TripsController : ControllerBase
//    {
//        private readonly ApplicationDbContext dbContext;

//        public TripsController(ApplicationDbContext dbcontext)
//        {
//            this.dbContext = dbcontext;
//        }

//        #region EndPoint_GetAllTrips
//        [HttpGet]
//        public async Task<IActionResult> GetAllTrips()
//        {
//            try
//            {
//                var trips = await dbContext.trips
//                    .Select(t => new Trip
//                    {
//                        TripId = t.TripId,
//                        TripName = t.TripName,
//                        Description = t.Description,
//                        PricePerPerson = t.PricePerPerson,
//                      //  DurationDays = t.DurationDays,
//                        StartDate = t.StartDate,
//                        EndDate = t.EndDate
//                    })
//                    .ToListAsync();

//                if (trips.Count == 0)
//                {
//                    return NotFound(new { statusCode = 404, message = "No trips found." });
//                }

//                return Ok(trips);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { statusCode = 500, message = "An error occurred while fetching trips.", error = ex.Message });
//            }
//        }
//        #endregion

//        #region EndPoint_GetTripById
//        [HttpGet("{id}")]
//        public async Task<IActionResult> GetTrip(int id)
//        {
//            try
//            {
//                var trip = await dbContext.trips
//                    .Where(t => t.TripId == id)
//                    .Select(t => new Trip
//                    {
//                        TripId = t.TripId,
//                        TripName = t.TripName,
//                        Description = t.Description,
//                        PricePerPerson = t.PricePerPerson,
//                       // DurationDays = t.DurationDays,
//                        StartDate = t.StartDate,
//                        EndDate = t.EndDate
//                    })
//                    .FirstOrDefaultAsync();

//                if (trip == null)
//                {
//                    return NotFound(new { statusCode = 404, message = "Trip not found." });
//                }

//                return Ok(trip);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { statusCode = 500, message = "An error occurred while fetching the trip.", error = ex.Message });
//            }
//        }

//        #endregion

//        //#region EndPoint_CreateTrip
//        //// POST: api/trips
//        //[HttpPost]
//        //public async Task<IActionResult> CreateTrip([FromBody] Trip tripDto)
//        //{
//        //    try
//        //    {
//        //        if (tripDto == null)
//        //        {
//        //            return BadRequest(new { statusCode = 400, message = "Invalid trip data." });
//        //        }

//        //        var trip = new Trip
//        //        {
//        //            TripName = tripDto.TripName,
//        //            Description = tripDto.Description,
//        //            PricePerPerson = tripDto.PricePerPerson,
//        //           // DurationDays = tripDto.DurationDays,
//        //            StartDate = tripDto.StartDate,
//        //            EndDate = tripDto.EndDate
//        //        };

//        //        dbContext.trips.Add(trip);
//        //        await dbContext.SaveChangesAsync();

//        //        return CreatedAtAction(nameof(GetTrip), new { id = trip.TripId }, trip);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        return StatusCode(500, new { statusCode = 500, message = "An error occurred while creating the trip.", error = ex.Message });
//        //    }
//        //}

//        //#endregion

//        #region EndPoint_UpdateTrip
//        [HttpPut("{id}")]
//        public async Task<IActionResult> UpdateTrip(int id, [FromBody] Trip tripDto)
//        {
//            try
//            {
//                var trip = await dbContext.trips.FindAsync(id);
//                if (trip == null)
//                {
//                    return NotFound(new { statusCode = 404, message = "Trip not found." });
//                }

//                trip.TripName = tripDto.TripName;
//                trip.Description = tripDto.Description;
//                trip.PricePerPerson = tripDto.PricePerPerson;
//                //trip.DurationDays = tripDto.DurationDays;
//                trip.StartDate = tripDto.StartDate;
//                trip.EndDate = tripDto.EndDate;

//                dbContext.trips.Update(trip);
//                await dbContext.SaveChangesAsync();

//                return NoContent();
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { statusCode = 500, message = "An error occurred while updating the trip.", error = ex.Message });
//            }
//        }

//        #endregion

//        #region EndPoint_DeleteTrip
//        // DELETE: api/trips/5
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> DeleteTrip(int id)
//        {
//            try
//            {
//                var trip = await dbContext.trips.FindAsync(id);
//                if (trip == null)
//                {
//                    return NotFound(new { statusCode = 404, message = "Trip not found." });
//                }

//                dbContext.trips.Remove(trip);
//                await dbContext.SaveChangesAsync();

//                return NoContent();
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { statusCode = 500, message = "An error occurred while deleting the trip.", error = ex.Message });
//            }
//        }

//        #endregion
//    }
//}
