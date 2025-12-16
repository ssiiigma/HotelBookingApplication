using Microsoft.EntityFrameworkCore;
using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Domain.Entities;
using HotelBookingApp.Infrastructure.Data;

namespace HotelBookingApp.Infrastructure.Repositories
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<ApplicationUser?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }
        
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.Email == email);
        }
    }
}