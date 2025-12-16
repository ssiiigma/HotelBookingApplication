using HotelBookingApp.Application.DTOs;
using HotelBookingApp.Domain.Entities;

namespace HotelBookingApp.Application.Interfaces
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetBookingsByRoomAsync(
            int roomId,
            DateTime checkIn,
            DateTime checkOut);

        Task<IEnumerable<Booking>> GetAllBookingsWithDetailsAsync();
        Task<BookingStatsDto> GetBookingStatisticsAsync(DateTime start, DateTime end);
    }
}