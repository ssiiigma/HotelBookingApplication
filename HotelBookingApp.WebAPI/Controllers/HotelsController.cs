using HotelBookingApp.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using CreateHotelRequest = HotelBookingApp.Application.Interfaces.CreateHotelRequest;
using UpdateHotelRequest = HotelBookingApp.Application.Interfaces.UpdateHotelRequest;

namespace HotelBookingApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly IApplicationDbContext _context;
        
        public HotelsController(IHotelService hotelService, IApplicationDbContext context)
        {
            _hotelService = hotelService;
            _context = context;
        }
        
        public async Task<ApiResponse> GetUserBookingsAsync(string userId)
        {
            try
            {
                var bookings = await _context.Bookings
                    .Include(b => b.Room)
                    .ThenInclude(r => r.Hotel)
                    .Where(b => b.UserId == userId)
                    .ToListAsync();

                if (bookings.Count == 0)
                    return new ApiResponse(false, "No bookings found for this user");

                return new ApiResponse(true, "Bookings loaded successfully", bookings);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error loading bookings: {ex.Message}");
            }
        }

        public async Task<ApiResponse> CreateBookingAsync(Booking booking)
        {
            try
            {
                var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == booking.RoomId && r.IsAvailable);
                if (room == null)
                    return new ApiResponse(false, "Room not available");

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                return new ApiResponse(true, "Booking created successfully", booking);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error creating booking: {ex.Message}");
            }
        }
        
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllHotels()
        {
            var response = await _hotelService.GetAllHotelsAsync();
            return Ok(response);
        }

        [HttpGet("{id}/rooms")]
        public async Task<IActionResult> GetRoomsByHotelId(int id)
        {
            var query = _context.Hotels
                .Include(h => h.Rooms)
                .Where(h => h.Id == id && h.IsActive);


            var hotel = await query.FirstOrDefaultAsync();
            
            if (hotel == null)
                return NotFound(new ApiResponse(false, "Hotel not found"));

            var rooms = hotel.Rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                RoomType = r.RoomType,
                RoomNumber = r.RoomNumber,
                PricePerNight = r.PricePerNight,
                Capacity = r.Capacity,
                Description = r.Description,
                IsAvailable = r.IsAvailable
            }).ToList();

            return Ok(new ApiResponse(true, "Rooms loaded", rooms));
        }


        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string city,
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut)
        {
            var result = await _hotelService.SearchAsync(city, checkIn, checkOut);
            return Ok(result);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateHotel([FromBody] CreateHotelRequest request)
        {
            var response = await _hotelService.CreateHotelAsync(request);
            return CreatedAtAction(nameof(GetRoomsByHotelId), new { id = response.Data?.Id }, response);
        }
        
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] UpdateHotelRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID mismatch");
            }
            
            var response = await _hotelService.UpdateHotelAsync(request);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var response = await _hotelService.DeleteHotelAsync(id);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }
    }
}