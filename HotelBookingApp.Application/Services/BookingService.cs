using HotelBookingApp.Application.Common;
using HotelBookingApp.Application.DTOs;
using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Domain.Entities;
using HotelBookingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApp.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IBookingService _bookingServiceImplementation;

        public BookingService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
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
        
        
        public async Task<Response<BookingDto>> CreateBookingAsync(
            string userId,
            CompleteBookingRequest request)
        {
            try
            {
                var room = await _unitOfWork.Rooms.GetByIdAsync(request.RoomId);

                if (room == null || !room.IsAvailable)
                {
                    return Response<BookingDto>.Fail("Room not available");
                }

                if (request.NumberOfGuests > room.Capacity)
                {
                    return Response<BookingDto>.Fail(
                        $"Room capacity is {room.Capacity} guests");
                }

                var existingBookings =
                    await _unitOfWork.Bookings.GetBookingsByRoomAsync(
                        request.RoomId,
                        request.CheckInDate,
                        request.CheckOutDate);

                if (existingBookings.Any(b => b.Status == BookingStatus.Confirmed))
                {
                    return Response<BookingDto>.Fail(
                        "Room is already booked for the selected dates");
                }

                if (request.GuestDetails.Count != request.NumberOfGuests)
                {
                    return Response<BookingDto>.Fail(
                        "Number of guest details must match number of guests");
                }

                if (!request.GuestDetails.Any(g => g.IsPrimary))
                {
                    return Response<BookingDto>.Fail(
                        "Primary guest must be specified");
                }

                var nights = (request.CheckOutDate - request.CheckInDate).Days;
                var totalPrice = room.PricePerNight * nights;

                var booking = new Booking
                {
                    UserId = userId,
                    RoomId = request.RoomId,
                    CheckInDate = request.CheckInDate,
                    CheckOutDate = request.CheckOutDate,
                    GuestsCount = request.NumberOfGuests,
                    TotalPrice = totalPrice,
                    Status = BookingStatus.Confirmed,
                    BookingDate = DateTime.UtcNow
                };

                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                var fullBooking =
                    await _unitOfWork.Bookings.GetByIdAsync(booking.Id);

                return Response<BookingDto>.Ok(
                    MapToDto(fullBooking!),
                    "Booking created successfully");
            }
            catch (Exception ex)
            {
                return Response<BookingDto>.Fail(
                    $"Booking failed: {ex.Message}");
            }
        }

        

        public Task<Response<BookingDto>> CreateBookingAsync(int userId, CreateBookingRequest request)
        {
            return _bookingServiceImplementation.CreateBookingAsync(userId, request);
        }
        


        public Task<Response<IEnumerable<BookingDto>>> GetUserBookingsAsync(int userId)
        {
            return _bookingServiceImplementation.GetUserBookingsAsync(userId);
        }

        public async Task<Response<IEnumerable<BookingDto>>> GetAllBookingsAsync()
        {
            var bookings =
                await _unitOfWork.Bookings.GetAllBookingsWithDetailsAsync();

            var bookingDtos =
                bookings.Select(MapToDto).ToList();

            return Response<IEnumerable<BookingDto>>.Ok(bookingDtos);
        }

        public Task<Response<bool>> CancelBookingAsync(int bookingId, int userId)
        {
            return _bookingServiceImplementation.CancelBookingAsync(bookingId, userId);
        }

        public async Task<Response<bool>> CancelBookingAsync(
            int bookingId,
            string userId)
        {
            var booking =
                await _unitOfWork.Bookings.GetByIdAsync(bookingId);

            if (booking == null)
            {
                return Response<bool>.Fail("Booking not found");
            }

            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Response<bool>.Fail("User not found");
            }

            var isAdmin =
                await _userManager.IsInRoleAsync(user, UserRoles.Admin);

            if (booking.UserId != userId && !isAdmin)
            {
                return Response<bool>.Fail(
                    "Unauthorized to cancel this booking");
            }

            booking.Status = BookingStatus.Cancelled;

            await _unitOfWork.Bookings.UpdateAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            return Response<bool>.Ok(
                true,
                "Booking cancelled successfully");
        }

        public async Task<Response<BookingStatsDto>> GetBookingStatisticsAsync(
            DateTime startDate,
            DateTime endDate)
        {
            var stats =
                await _unitOfWork.Bookings.GetBookingStatisticsAsync(
                    startDate,
                    endDate);

            return Response<BookingStatsDto>.Ok(stats);
        }

        private static BookingDto MapToDto(Booking booking)
        {
            return new BookingDto
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                RoomNumber = booking.Room?.RoomNumber ?? string.Empty,
                HotelName = booking.Room?.Hotel?.Name ?? string.Empty,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                NumberOfGuests = booking.GuestsCount,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status.ToString(),
                CreatedAt = booking.BookingDate
            };
        }
    }
}
