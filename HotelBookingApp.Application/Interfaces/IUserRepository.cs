using HotelBookingApp.Domain.Entities;

namespace HotelBookingApp.Application.Interfaces
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
    }
}