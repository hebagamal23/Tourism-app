using Microsoft.EntityFrameworkCore;
using Tourism_project.Models;

namespace Tourism_project.Services
{
    public class RoomAvailabilityService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RoomAvailabilityService> _logger;

        public RoomAvailabilityService(IServiceScopeFactory scopeFactory, ILogger<RoomAvailabilityService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    try
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var now = DateTime.Now;

                        var roomsToUpdate = await dbContext.bookings
                            .Include(b => b.Room)
                            .Where(b => b.StartDate.Date == now.Date &&
                                        b.StartDate <= now &&
                                        b.Room.IsAvailable == true)
                            .Select(b => b.Room)
                            .ToListAsync();

                        foreach (var room in roomsToUpdate)
                        {
                            room.IsAvailable = false;
                            _logger.LogInformation($"Room {room.Id} has been marked as unavailable.");
                        }

                        if (roomsToUpdate.Any())
                        {
                            await dbContext.SaveChangesAsync();
                            _logger.LogInformation($"{roomsToUpdate.Count} room(s) updated successfully.");
                        }
                        else
                        {
                            _logger.LogInformation("No rooms needed updating.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error in room availability service: {ex.Message}");
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
