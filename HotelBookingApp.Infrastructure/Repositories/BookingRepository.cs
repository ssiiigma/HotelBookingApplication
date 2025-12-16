using HotelBookingApp.Application.DTOs;
using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Domain.Entities;
using HotelBookingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApp.Infrastructure.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId)
        {
            return await _context.Bookings!
                .Include(b => b.Room)
                    .ThenInclude(r => r.Hotel)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByRoomAsync(
            int roomId,
            DateTime checkIn,
            DateTime checkOut)
        {
            return await _context.Bookings!
                .Where(b =>
                    b.RoomId == roomId &&
                    b.CheckInDate < checkOut &&
                    b.CheckOutDate > checkIn)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsWithDetailsAsync()
        {
            return await _context.Bookings!
                .Include(b => b.User)
                .Include(b => b.Room)
                    .ThenInclude(r => r.Hotel)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<BookingStatsDto> GetBookingStatisticsAsync(
            DateTime start,
            DateTime end)
        {
            var bookings = await _context.Bookings!
                .Where(b => b.BookingDate >= start && b.BookingDate <= end)
                .ToListAsync();

            return new BookingStatsDto
            {
                TotalBookings = bookings.Count,
                TotalRevenue = bookings.Sum(b => b.TotalPrice),
                CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled)
            };
        }
    }
}
