using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelBookingApp.Application.DTOs;
using HotelBookingApp.Application.Interfaces;
using CreateRoomRequest = HotelBookingApp.Application.Interfaces.CreateRoomRequest;

namespace HotelBookingApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;
        
        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }
        
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchRooms([FromQuery] RoomSearchRequest request)
        {
            var response = await _roomService.GetAvailableRoomsAsync(request);
            return Ok(response);
        }
        
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var response = await _roomService.GetRoomByIdAsync(id);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }
        
        [HttpGet("hotel/{hotelId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoomsByHotelId(int hotelId)
        {
            var response = await _roomService.GetRoomsByHotelIdAsync(hotelId);
            return Ok(response);
        }
        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            var response = await _roomService.CreateRoomAsync(request);
            return CreatedAtAction(nameof(GetRoomById), new { id = response.Data?.Id }, response);
        }
        
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] CreateRoomRequest request)
        {
            var response = await _roomService.UpdateRoomAsync(id, request);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var response = await _roomService.DeleteRoomAsync(id);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }
    }
}