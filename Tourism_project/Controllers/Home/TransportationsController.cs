//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Tourism_project.Models;

//namespace Tourism_project.Controllers.Home
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class TransportationsController : ControllerBase
//    {
//        private readonly ApplicationDbContext dbContext;

//        public TransportationsController(ApplicationDbContext dbContext)
//        {
//            this.dbContext = dbContext;
//        }

//        #region EndPoint_GetAllTransportations
//        [HttpGet("api/transportations")]
//        public async Task<IActionResult> GetAllTransportations()
//        {
//            try
//            {
//                var transportations = await dbContext.Transportations
//                    .Select(t => new Transportation
//                    {
//                        Id = t.Id,
//                        Name = t.Name,
//                        Type = t.Type.ToString(), // Assuming Type is an Enum
//                        FromLocationId = t.FromLocationId,
//                        ToLocationId = t.ToLocationId,
//                        DepartureTime = t.DepartureTime,
//                        ArrivalTime = t.ArrivalTime,
//                        Price = t.Price,
//                        Capacity = t.Capacity
//                    })
//                    .ToListAsync();

//                if (transportations.Count == 0)
//                {
//                    return NotFound(new { statusCode = 404, message = "No transportations found." });
//                }

//                return Ok(transportations);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { statusCode = 500, message = "An error occurred while fetching transportations.", error = ex.Message });
//            }
//        }

//        #endregion

//        #region EndPoint_GetTransportationById
//        [HttpGet("api/transportations/{id}")]
//        public async Task<IActionResult> GetTransportationById(int id)
//        {
//            try
//            {
//                var transportation = await dbContext.Transportations
//                    .Where(t => t.Id == id)
//                    .Select(t => new Transportation
//                    {
//                        Id = t.Id,
//                        Name = t.Name,
//                        Type = t.Type.ToString(), // Assuming Type is an Enum
//                        FromLocationId = t.FromLocationId,
//                        ToLocationId = t.ToLocationId,
//                        DepartureTime = t.DepartureTime,
//                        ArrivalTime = t.ArrivalTime,
//                        Price = t.Price,
//                        Capacity = t.Capacity
//                    })
//                    .FirstOrDefaultAsync();

//                if (transportation == null)
//                {
//                    return NotFound(new { statusCode = 404, message = "Transportation not found." });
//                }

//                return Ok(transportation);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { statusCode = 500, message = "An error occurred while fetching transportation.", error = ex.Message });
//            }
//        }

//        #endregion

//        #region EndPoint_GetTransportationBy/{locationId}

//        [HttpGet("GetTransportationBy/{locationId}")]
//        public async Task<IActionResult> GetTransportationByLocationId(int locationId)
//        {
//            try
//            {
//                // التحقق من وجود locationId في جدول TransportationLocation
//                var transportation = await dbContext.TransportationLocations
//                    .Where(tl => tl.LocationId == locationId)
//                    .Select(tl => tl.Transportation) // استرجاع وسائل النقل المرتبطة
//                    .ToListAsync();

//                // إذا لم يتم العثور على وسائل النقل
//                if (transportation == null || transportation.Count == 0)
//                {
//                    return NotFound(new
//                    {
//                        statusCode = 404,
//                        message = "No transportation available for this location."
//                    });
//                }

//                // إرجاع وسائل النقل
//                return Ok(new
//                {
//                    transportation
//                });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new
//                {
//                    statusCode = 500,
//                    message = "An error occurred while retrieving transportation data.",
//                    error = ex.Message
//                });
//            }
//        }

//        #endregion

//        //#region EndPoint_AddTransportation

//        //[HttpPost("api/transportations")]
//        //public async Task<IActionResult> AddTransportation([FromBody] Transportation transportationDto)
//        //{
//        //    try
//        //    {
//        //        if (transportationDto == null)
//        //        {
//        //            return BadRequest(new { statusCode = 400, message = "Invalid data." });
//        //        }

//        //        var transportation = new Transportation
//        //        {
//        //            Name = transportationDto.Name,
//        //            Type = transportationDto.Type, 
//        //            FromLocationId = transportationDto.FromLocationId,
//        //            ToLocationId = transportationDto.ToLocationId,
//        //            DepartureTime = transportationDto.DepartureTime,
//        //            ArrivalTime = transportationDto.ArrivalTime,
//        //            Price = transportationDto.Price,
//        //            Capacity = transportationDto.Capacity
//        //        };

//        //        dbContext.Transportations.Add(transportation);
//        //        await dbContext.SaveChangesAsync();

//        //        return CreatedAtAction(nameof(GetTransportationById), new { id = transportation.Id }, transportation);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        return StatusCode(500, new { statusCode = 500, message = "An error occurred while adding transportation.", error = ex.Message });
//        //    }
//        //}

//        //#endregion


//        #region EndPoint_UpdateTransportation
//        [HttpPut("api/transportations/{id}")]
//        public async Task<IActionResult> UpdateTransportation(int id, [FromBody] Transportation transportationDto)
//        {
//            try
//            {
//                if (transportationDto == null || id != transportationDto.Id)
//                {
//                    return BadRequest(new { statusCode = 400, message = "Invalid data." });
//                }

//                var transportation = await dbContext.Transportations.FindAsync(id);

//                if (transportation == null)
//                {
//                    return NotFound(new { statusCode = 404, message = "Transportation not found." });
//                }

//                transportation.Name = transportationDto.Name;
//                transportation.Type = transportationDto.Type; 
//                transportation.FromLocationId = transportationDto.FromLocationId;
//                transportation.ToLocationId = transportationDto.ToLocationId;
//                transportation.DepartureTime = transportationDto.DepartureTime;
//                transportation.ArrivalTime = transportationDto.ArrivalTime;
//                transportation.Price = transportationDto.Price;
//                transportation.Capacity = transportationDto.Capacity;

//                dbContext.Transportations.Update(transportation);
//                await dbContext.SaveChangesAsync();

//                return NoContent();
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { statusCode = 500, message = "An error occurred while updating transportation.", error = ex.Message });
//            }
//        }

//        #endregion


//        #region EndPoint_DeleteTransportation
//        [HttpDelete("api/transportations/{id}")]
//        public async Task<IActionResult> DeleteTransportation(int id)
//        {
//            try
//            {
//                var transportation = await dbContext.Transportations.FindAsync(id);

//                if (transportation == null)
//                {
//                    return NotFound(new { statusCode = 404, message = "Transportation not found." });
//                }

//                dbContext.Transportations.Remove(transportation);
//                await dbContext.SaveChangesAsync();

//                return NoContent();
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { statusCode = 500, message = "An error occurred while deleting transportation.", error = ex.Message });
//            }
//        }

//        #endregion
//    }
//}
