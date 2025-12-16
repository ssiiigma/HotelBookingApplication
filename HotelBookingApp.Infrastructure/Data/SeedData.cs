using HotelBookingApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IdentityRole = Microsoft.AspNetCore.Identity.IdentityRole;

namespace HotelBookingApp.Infrastructure.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.EnsureCreatedAsync();

            string[] roles = { "Admin", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@hotelbooking.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "System",
                    PhoneNumber = "+1234567890",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            if (!await context.Hotels.AnyAsync())
            {
                var hotels = new List<Hotel>
                {
                    new Hotel
                    {
                        Name = "Grand Plaza Hotel",
                        City = "Kyiv",
                        Address = "Khreschatyk St, 22",
                        Description = "Luxury hotel in the heart of Kyiv",
                        StarRating = 5,
                        PricePerNight = 4500,
                        MainImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945",
                        HasPool = true,
                        HasSpa = true,
                        HasRestaurant = true,
                        HasFreeWiFi = true,
                        HasParking = true,
                        IsActive = true,
                        Rooms = new List<Room>
                        {
                            new Room { Description = "Comfortable standard room", Capacity = 2, PricePerNight = 2500, HasBreakfast = true, HasAC = true, HasTV = true },
                            new Room { Description = "Spacious deluxe room", Capacity = 3, PricePerNight = 3500, HasBreakfast = true, HasAC = true, HasTV = true, HasMiniBar = true },
                            new Room { Description = "Luxury suite with view", Capacity = 4, PricePerNight = 5500, HasBreakfast = true, HasAC = true, HasTV = true, HasMiniBar = true, HasBalcony = true }
                        }
                    },
                    new Hotel
                    {
                        Name = "River View Hotel",
                        City = "Lviv",
                        Address = "Svobody Ave, 15",
                        Description = "Historic hotel in Lviv center",
                        StarRating = 4,
                        PricePerNight = 3200,
                        MainImageUrl = "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb",
                        HasPool = false,
                        HasSpa = true,
                        HasRestaurant = true,
                        HasFreeWiFi = true,
                        HasParking = true,
                        IsActive = true,
                        Rooms = new List<Room>
                        {
                            new Room { Description = "Cozy standard room", Capacity = 2, PricePerNight = 1800, HasBreakfast = true, HasAC = true, HasTV = true },
                            new Room { Description = "Deluxe room with city view", Capacity = 3, PricePerNight = 2800, HasBreakfast = true, HasAC = true, HasTV = true }
                        }
                    }
                };

                await context.Hotels.AddRangeAsync(hotels);
                await context.SaveChangesAsync();
            }
        }
    }
}