using HotelBookingApp.Domain.Entities;

namespace HotelBookingApp.Application.Interfaces
{
    public interface IHotelRepository : IRepository<Hotel>
    {
        Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city);
        Task<IEnumerable<Hotel>> SearchHotelsAsync(string? city, string? country);
    }
}