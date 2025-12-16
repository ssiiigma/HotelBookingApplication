using HotelBookingApp.Application.Common;
using HotelBookingApp.Application.DTOs;

namespace HotelBookingApp.Application.Interfaces
{
    public interface IBookingService
    {
        Task<Response<BookingDto>> CreateBookingAsync(int userId, CreateBookingRequest request);
        Task<Response<IEnumerable<BookingDto>>> GetUserBookingsAsync(int userId);
        Task<Response<bool>> CancelBookingAsync(int bookingId, int userId);
    }
}