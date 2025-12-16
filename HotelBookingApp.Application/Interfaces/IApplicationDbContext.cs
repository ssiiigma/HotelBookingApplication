// Services/IApplicationDbContext.cs

using HotelBookingApp.Domain.Entities;
using HotelBookingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApp.Application.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Hotel> Hotels { get; set; }
        DbSet<Room> Rooms { get; set; }
        DbSet<Booking> Bookings { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}