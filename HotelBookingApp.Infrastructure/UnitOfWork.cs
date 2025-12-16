using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Domain.Entities;
using HotelBookingApp.Infrastructure.Data;
using HotelBookingApp.Infrastructure.Repositories;

namespace HotelBookingApp.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Hotels = new HotelRepository(context);
            Rooms = new RoomRepository(context);
            Bookings = new BookingRepository(context);
            Users = new UserRepository(context);
        }
        
        public IHotelRepository Hotels { get; private set; }
        public IRoomRepository Rooms { get; private set; }
        public IBookingRepository Bookings { get; private set; }
        public IUserRepository Users { get; private set; }

        
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        
        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}