using HotelBookingApp.Domain.Entities;
using HotelBookingApp.Models;

namespace HotelBookingApp.Application.Interfaces
{
    public interface IRoomRepository : IRepository<Room>
    {
        Task<IEnumerable<Room>> GetAvailableRoomsAsync(int hotelId, DateTime checkIn, DateTime checkOut);
        Task<IEnumerable<Room>> SearchAvailableRoomsAsync(string city, DateTime checkIn, DateTime checkOut, int guests);
        Task<IEnumerable<Room>> GetRoomsByHotelIdAsync(int hotelId);
    }
}