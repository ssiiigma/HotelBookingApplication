using HotelBookingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingApp.Controllers.Api
{
    [Route("api/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelService _hotelService;
        private readonly ILogger<HotelsController> _logger;

        public HotelsController(IHotelService hotelService, ILogger<HotelsController> logger)
        {
            _hotelService = hotelService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllHotels()
        {
            var response = await _hotelService.GetAllHotelsAsync();
            
            if (!response.Success)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHotelById(int id)
        {
            var response = await _hotelService.GetHotelByIdAsync(id);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHotel([FromBody] CreateHotelRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _hotelService.CreateHotelAsync(request);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }
            
            return CreatedAtAction(nameof(GetHotelById), new { id = response.Data?.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHotel(int id, [FromBody] UpdateHotelRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

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
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var response = await _hotelService.DeleteHotelAsync(id);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }


        [HttpGet("rooms")]
        public async Task<IActionResult> GetAllRooms([FromQuery] int? hotelId = null)
        {
            var response = await _hotelService.GetAllRoomsAsync(hotelId);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }

        [HttpGet("rooms/{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            var response = await _hotelService.GetRoomByIdAsync(id);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }

        [HttpPost("rooms")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _hotelService.CreateRoomAsync(request);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }
            
            return CreatedAtAction(nameof(GetRoomById), new { id = response.Data?.Id }, response);
        }

        [HttpPut("rooms/{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != request.Id)
            {
                return BadRequest("ID mismatch");
            }
            
            var response = await _hotelService.UpdateRoomAsync(request);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }

        [HttpDelete("rooms/{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var response = await _hotelService.DeleteRoomAsync(id);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }

        [HttpPatch("rooms/{id}/availability")]
        public async Task<IActionResult> UpdateRoomAvailability(int id, [FromBody] UpdateAvailabilityRequest request)
        {
            var response = await _hotelService.UpdateRoomAvailabilityAsync(id, request.IsAvailable);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }
    }

    public class UpdateAvailabilityRequest
    {
        public bool IsAvailable { get; set; }
    }
}