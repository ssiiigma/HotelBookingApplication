using HotelBookingApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IHotelService hotelService, ILogger<HomeController> logger)
        {
            _hotelService = hotelService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<ActionResult<WebAPI.Controllers.ApiResponse>> Search(
            [FromQuery] string? city = null,
            [FromQuery] DateTime? checkIn = null,
            [FromQuery] DateTime? checkOut = null)
        {
            try
            {
                var searchCheckIn = checkIn ?? DateTime.Today.AddDays(1);
                var searchCheckOut = checkOut ?? searchCheckIn.AddDays(1);

                if (searchCheckOut <= searchCheckIn)
                {
                    return BadRequest(new WebAPI.Controllers.ApiResponse(false, "Check-out date must be after check-in date"));
                }

                var result = await _hotelService.SearchAsync(city ?? "", searchCheckIn, searchCheckOut);

                return Ok(new WebAPI.Controllers.ApiResponse(true, "Search completed successfully", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching hotels");
                return StatusCode(500, new WebAPI.Controllers.ApiResponse(false, "Error searching hotels"));
            }
        }

        [HttpGet("hotel/{id}")]
        public async Task<ActionResult<WebAPI.Controllers.ApiResponse>> GetHotelDetails(int id)
        {
            try
            {
                var hotelDetails = await _hotelService.GetHotelDetailsAsync(id);
                return Ok(new WebAPI.Controllers.ApiResponse(true, "Hotel details retrieved", hotelDetails));
            }
            catch (ArgumentException ex)
            {
                return NotFound(new WebAPI.Controllers.ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hotel details for ID {HotelId}", id);
                return StatusCode(500, new WebAPI.Controllers.ApiResponse(false, "Error getting hotel details"));
            }
        }

        [HttpGet("hotel/{hotelId}/rooms/available")]
        public async Task<ActionResult<WebAPI.Controllers.ApiResponse>> GetAvailableRooms(
            int hotelId,
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut)
        {
            try
            {
                if (checkOut <= checkIn)
                {
                    return BadRequest(new WebAPI.Controllers.ApiResponse(false, "Check-out date must be after check-in date"));
                }

                var availableRooms = await _hotelService.GetAvailableRoomsAsync(hotelId, checkIn, checkOut);
                
                return Ok(new WebAPI.Controllers.ApiResponse(true, "Available rooms retrieved", availableRooms));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new WebAPI.Controllers.ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available rooms for hotel ID {HotelId}", hotelId);
                return StatusCode(500, new WebAPI.Controllers.ApiResponse(false, "Error getting available rooms"));
            }
        }

        [HttpGet("room/{roomId}/availability")]
        public async Task<ActionResult<WebAPI.Controllers.ApiResponse>> CheckRoomAvailability(
            int roomId,
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut)
        {
            try
            {
                if (checkOut <= checkIn)
                {
                    return BadRequest(new WebAPI.Controllers.ApiResponse(false, "Check-out date must be after check-in date"));
                }

                var isAvailable = await _hotelService.IsRoomAvailableAsync(roomId, checkIn, checkOut);
                
                return Ok(new WebAPI.Controllers.ApiResponse(true, "Availability checked", new { IsAvailable = isAvailable }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for room ID {RoomId}", roomId);
                return StatusCode(500, new WebAPI.Controllers.ApiResponse(false, "Error checking room availability"));
            }
        }

        [HttpGet("featured")]
        public async Task<ActionResult<WebAPI.Controllers.ApiResponse>> GetFeaturedHotels()
        {
            try
            {
                var checkIn = GetNextFriday();
                var checkOut = checkIn.AddDays(2); 

                var cities = new[] { "Kyiv", "Lviv", "Odesa", "Kharkiv" };
                var featuredHotels = new List<object>();

                foreach (var city in cities)
                {
                    try
                    {
                        var hotels = await _hotelService.SearchAsync(city, checkIn, checkOut);
                        var topHotel = hotels.OrderByDescending(h => h.StarRating)
                                           .ThenByDescending(h => h.AverageRating)
                                           .FirstOrDefault();
                        
                        if (topHotel != null)
                        {
                            featuredHotels.Add(topHotel);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error getting featured hotels for city {City}", city);
                    }
                }

                return Ok(new WebAPI.Controllers.ApiResponse(true, "Featured hotels retrieved", featuredHotels));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured hotels");
                return StatusCode(500, new WebAPI.Controllers.ApiResponse(false, "Error getting featured hotels"));
            }
        }

        private DateTime GetNextFriday()
        {
            var today = DateTime.Today;
            var daysUntilFriday = ((int)DayOfWeek.Friday - (int)today.DayOfWeek + 7) % 7;
            return today.AddDays(daysUntilFriday == 0 ? 7 : daysUntilFriday);
        }
    }
}