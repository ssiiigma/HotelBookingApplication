using Microsoft.EntityFrameworkCore;
using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Domain.Entities;
using HotelBookingApp.Infrastructure.Data;

namespace HotelBookingApp.Infrastructure.Repositories
{
    public class RoomRepository : Repository<Room>, IRoomRepository
    {
        public RoomRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<IEnumerable<Room>> GetAvailableRoomsAsync(int hotelId, DateTime checkIn, DateTime checkOut)
        {
            return await _dbSet
                .Include(r => r.Hotel)
                .Where(r => r.HotelId == hotelId && r.IsAvailable)
                .Where(r => !r.Bookings.Any(b => 
                    b.Status != BookingStatus.Cancelled &&
                    b.CheckInDate < checkOut && 
                    b.CheckOutDate > checkIn))
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Room>> SearchAvailableRoomsAsync(string city, DateTime checkIn, DateTime checkOut, int guests)
        {
            return await _dbSet
                .Include(r => r.Hotel)
                .Where(r => r.Hotel.City == city && 
                            r.IsAvailable && 
                            r.Capacity >= guests)
                .Where(r => !r.Bookings.Any(b => 
                    b.Status != BookingStatus.Cancelled &&
                    b.CheckInDate < checkOut && 
                    b.CheckOutDate > checkIn))
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Room>> GetRoomsByHotelIdAsync(int hotelId)
        {
            return await _dbSet
                .Include(r => r.Hotel)
                .Where(r => r.HotelId == hotelId)
                .ToListAsync();
        }
    }
}