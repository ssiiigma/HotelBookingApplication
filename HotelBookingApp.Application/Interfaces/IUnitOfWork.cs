namespace HotelBookingApp.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IHotelRepository Hotels { get; }
        IRoomRepository Rooms { get; }
        IBookingRepository Bookings { get; }
        IUserRepository Users { get; }
        Task<int> SaveChangesAsync();
    }
}