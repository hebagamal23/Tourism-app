using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tourism_project.Dtos.Home;
using Tourism_project.Models;

namespace Tourism_project.Controllers.Home
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiciesController : ControllerBase
    {

        private readonly ApplicationDbContext dbContext;

        public ServiciesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        [HttpGet("services/{hotelId}")]
        public async Task<IActionResult> GetServicesByHotelId(int hotelId)
        {
            
            var hotelExists = await dbContext.Hotels.AnyAsync(h => h.HotelId == hotelId);
            if (!hotelExists)
            {
                return NotFound(new
                {
                    StatusCode = 404,
                    Message = $"Hotel with ID {hotelId} was not found."
                });
            }

         
            var services = await dbContext.HotelServices
                .Where(hs => hs.HotelId == hotelId)
                .Select(hs => new ServicesDto
                {
                    ServiceId = hs.Service.ServiceId,
                    Name = hs.Service.Name,
                    IconName = hs.Service.IconName,
                    IconType = hs.Service.IconType,
                }).ToListAsync();

            return Ok(new
            {
                StatusCode = 200,
                Message = "Services retrieved successfully.",
                Data = services
            });
        }


    }
}
