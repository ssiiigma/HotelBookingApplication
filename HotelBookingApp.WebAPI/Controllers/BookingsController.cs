using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using HotelBookingApp.Application.Services;
using HotelBookingApp.Domain.Entities;
using HotelBookingApp.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<BookingController> _logger;
        private readonly BookingService _bookingService; 

        public BookingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<BookingController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("calculate-price/{roomId}")]
        public async Task<ActionResult<ApiResponse>> CalculatePrice(
            int roomId,
            [FromQuery] DateTime checkIn,
            [FromQuery] DateTime checkOut,
            [FromQuery] int guests = 1)
        {
            try
            {
                var room = await _context.Rooms
                    .Include(r => r.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == roomId);

                if (room == null)
                {
                    return NotFound(new ApiResponse(false, "Room not found"));
                }

                if (checkOut <= checkIn || checkIn < DateTime.Today)
                {
                    return BadRequest(new ApiResponse(false, "Invalid dates selected"));
                }

                var isAvailable = await IsRoomAvailableAsync(roomId, checkIn, checkOut);
                if (!isAvailable)
                {
                    return BadRequest(new ApiResponse(false, "Room not available for selected dates"));
                }

                var nights = (int)(checkOut - checkIn).TotalDays;
                var totalPrice = room.PricePerNight * nights;

                var response = new
                {
                    RoomId = roomId,
                    HotelName = room.Hotel.Name,
                    CheckIn = checkIn,
                    CheckOut = checkOut,
                    Guests = guests,
                    Nights = nights,
                    PricePerNight = room.PricePerNight,
                    TotalPrice = totalPrice,
                    IsAvailable = isAvailable
                };

                return Ok(new ApiResponse(true, "Price calculated successfully", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating price for room {RoomId}", roomId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse>> CreateBooking([FromBody] CreateBookingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, "Invalid request data", ModelState));
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new ApiResponse(false, "User not found"));
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Contains("Customer") && !userRoles.Contains("Admin"))
                {
                    return Forbid();
                }

                var room = await _context.Rooms
                    .Include(r => r.Hotel)
                    .FirstOrDefaultAsync(r => r.Id == request.RoomId);

                if (room == null)
                {
                    return NotFound(new ApiResponse(false, "Room not found"));
                }

                if (request.CheckOutDate <= request.CheckInDate || request.CheckInDate < DateTime.Today)
                {
                    return BadRequest(new ApiResponse(false, "Invalid dates selected"));
                }

                var isAvailable = await IsRoomAvailableAsync(request.RoomId, request.CheckInDate, request.CheckOutDate);
                if (!isAvailable)
                {
                    return BadRequest(new ApiResponse(false, "Room not available for selected dates"));
                }

                var nights = (int)(request.CheckOutDate - request.CheckInDate).TotalDays;
                var totalPrice = room.PricePerNight * nights;

                var booking = new Booking
                {
                    UserId = user.Id,
                    RoomId = request.RoomId,
                    CheckInDate = request.CheckInDate,
                    CheckOutDate = request.CheckOutDate,
                    GuestsCount = request.GuestsCount,
                    TotalPrice = totalPrice,
                    SpecialRequests = request.SpecialRequests,
                    BookingDate = DateTime.UtcNow,
                    Status = BookingStatus.Confirmed
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                var response = new
                {
                    BookingId = booking.Id,
                    Booking = new
                    {
                        booking.Id,
                        booking.CheckInDate,
                        booking.CheckOutDate,
                        booking.GuestsCount,
                        booking.TotalPrice,
                        booking.Status,
                        booking.BookingDate,
                        Room = new
                        {
                            room.PricePerNight
                        },
                        Hotel = new
                        {
                            room.Hotel.Name,
                            room.Hotel.City
                        }
                    }
                };

                return Ok(new ApiResponse(true, "Booking created successfully", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(false, "User not authenticated"));

            var response = await _bookingService.GetUserBookingsAsync(userId);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] Booking booking)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ApiResponse(false, "User not authenticated"));

            booking.UserId = userId;
            var response = await _bookingService.CreateBookingAsync(booking);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse>> GetBookingDetails(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new ApiResponse(false, "User not found"));
                }

                var booking = await _context.Bookings
                    .Include(b => b.Room)
                        .ThenInclude(r => r.Hotel)
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.Id == id && b.UserId == user.Id);

                if (booking == null)
                {
                    return NotFound(new ApiResponse(false, "Booking not found"));
                }

                var response = new
                {
                    booking.Id,
                    booking.CheckInDate,
                    booking.CheckOutDate,
                    booking.GuestsCount,
                    booking.TotalPrice,
                    booking.SpecialRequests,
                    booking.Status,
                    booking.BookingDate,
                    booking.UpdatedAt,
                    Room = new
                    {
                        booking.Room.Id,
                        booking.Room.Description,
                        booking.Room.Capacity,
                        booking.Room.PricePerNight,
                        booking.Room.RoomImageUrl
                    },
                    Hotel = new
                    {
                        booking.Room.Hotel.Id,
                        booking.Room.Hotel.Name,
                        booking.Room.Hotel.City,
                        booking.Room.Hotel.Address,
                        booking.Room.Hotel.MainImageUrl
                    }
                };

                return Ok(new ApiResponse(true, "Booking details retrieved successfully", response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching booking details");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<ApiResponse>> CancelBooking(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized(new ApiResponse(false, "User not found"));
                }

                var booking = await _context.Bookings
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (booking == null)
                {
                    return NotFound(new ApiResponse(false, "Booking not found"));
                }

                if (booking.UserId != user.Id)
                {
                    return Forbid();
                }

                if (booking.CheckInDate <= DateTime.UtcNow.AddHours(24))
                {
                    return BadRequest(new ApiResponse(false, "Booking can only be cancelled at least 24 hours before check-in"));
                }

                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse(true, "Booking cancelled successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        private async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            return !await _context.Bookings.AnyAsync(b =>
                b.RoomId == roomId &&
                b.Status != BookingStatus.Cancelled &&
                ((checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                 (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                 (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)));
        }
    }

    public class CreateBookingRequest
    {
        [Required]
        public int RoomId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, 10)]
        public int GuestsCount { get; set; }

        [StringLength(500)]
        public string? SpecialRequests { get; set; }
    }
}