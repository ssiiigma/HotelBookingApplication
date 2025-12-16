using Microsoft.EntityFrameworkCore;
using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Domain.Entities;
using HotelBookingApp.Infrastructure.Data;

namespace HotelBookingApp.Infrastructure.Repositories
{
    public class HotelRepository : Repository<Hotel>, IHotelRepository
    {
        public HotelRepository(ApplicationDbContext context) : base(context)
        {
        }
        
        public async Task<IEnumerable<Hotel>> GetHotelsByCityAsync(string city)
        {
            return await _dbSet
                .Include(h => h.Rooms)
                .Where(h => h.City == city)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Hotel>> SearchHotelsAsync(string? city, string? country)
        {
            var query = _dbSet.Include(h => h.Rooms).AsQueryable();
            
            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(h => h.City.Contains(city));
            }
            
            return await query.ToListAsync();
        }
    }
}