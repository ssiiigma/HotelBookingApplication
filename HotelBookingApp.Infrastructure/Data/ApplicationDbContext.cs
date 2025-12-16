using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Domain.Entities;
using HotelBookingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HotelBookingApp.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<IdentityRole> Roles { get; set; }
        public DbSet<IdentityUserRole<string>> UserRoles { get; set; }

        public IQueryable<TEntity> Query<TEntity>() where TEntity : class
        {
            return Set<TEntity>();
        }

        public async Task<ApplicationUser> FindUserByEmailAsync(string email)
        {
            return await Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string role)
        {
            throw new NotImplementedException();
        }

        public Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            throw new NotImplementedException();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Hotel>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PricePerNight).HasColumnType("decimal(18,2)");
                
                entity.HasMany(h => h.Rooms)
                    .WithOne(r => r.Hotel)
                    .HasForeignKey(r => r.HotelId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RoomNumber).IsRequired().HasMaxLength(10);
                entity.Property(e => e.RoomType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PricePerNight).HasColumnType("decimal(18,2)");
            });

            builder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
                
                entity.HasOne(b => b.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(b => b.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
    
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (!context.Hotels.Any())
            {
                var hotels = new List<Hotel>
                {
                    new Hotel
                    {
                        Name = "Grand Hotel Kyiv",
                        City = "Kyiv",
                        Address = "Khreshchatyk 1",
                        Description = "Luxury hotel in the center of Kyiv",
                        StarRating = 5,
                        PricePerNight = 150,
                        MainImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945",
                        HasPool = true,
                        HasSpa = true,
                        HasRestaurant = true,
                        HasFreeWiFi = true,
                        HasParking = true,
                        IsActive = true,
                        Rooms = new List<Room>
                        {
                            new Room { RoomType = "Standard", RoomNumber = "101", PricePerNight = 150, Capacity = 2, Description = "Cozy standard room", HasAC = true, HasBreakfast = true, HasTV = true },
                            new Room { RoomType = "Deluxe", RoomNumber = "102", PricePerNight = 200, Capacity = 3, Description = "Spacious deluxe room with city view", HasAC = true, HasBreakfast = true, HasTV = true, HasMiniBar = true, HasBalcony = true },
                            new Room { RoomType = "Suite", RoomNumber = "201", PricePerNight = 250, Capacity = 4, Description = "Luxury suite with lounge", HasAC = true, HasBreakfast = true, HasTV = true, HasMiniBar = true, HasBalcony = true }
                        }
                    },
                    new Hotel
                    {
                        Name = "Lviv Palace",
                        City = "Lviv",
                        Address = "Rynok Square 5",
                        Description = "Historic hotel near Rynok Square",
                        StarRating = 4,
                        PricePerNight = 120,
                        MainImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945",
                        HasPool = false,
                        HasSpa = true,
                        HasRestaurant = true,
                        HasFreeWiFi = true,
                        HasParking = true,
                        IsActive = true,
                        Rooms = new List<Room>
                        {
                            new Room { RoomType = "Standard", RoomNumber = "101", PricePerNight = 120, Capacity = 2, Description = "Comfortable room near Rynok Square", HasAC = true, HasBreakfast = true, HasTV = true },
                            new Room { RoomType = "Deluxe", RoomNumber = "102", PricePerNight = 150, Capacity = 3, Description = "Spacious room with balcony", HasAC = true, HasBreakfast = true, HasTV = true, HasMiniBar = true, HasBalcony = true }
                        }
                    }
                };

                context.Hotels.AddRange(hotels);
                context.SaveChanges();
            }
        }
    }

}