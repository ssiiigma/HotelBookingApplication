using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Application.Services;
using HotelBookingApp.Domain.Entities;
using HotelBookingApp.Infrastructure.Data;
using HotelBookingApp.Models;
using HotelBookingApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:10000");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

/*builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 26))
    )
    .EnableSensitiveDataLogging()
    .EnableDetailedErrors()
);*/

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions =>
        {
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null
            );
        }
    ));


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IApplicationDbContext>(provider => 
    provider.GetRequiredService<ApplicationDbContext>());

builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IHotelService, HotelService>();


builder.Services.AddHttpContextAccessor();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/api/account/login";
    options.LogoutPath = "/api/account/logout";
    options.AccessDeniedPath = "/api/account/accessdenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await ApplicationDbContextSeed.SeedAsync(context);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}


app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    try
    {
        context.Database.EnsureCreated();
        
        if (!context.Hotels.Any())
        {
            Console.WriteLine("Seeding database with initial data...");
            
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!await roleManager.RoleExistsAsync("Customer"))
                await roleManager.CreateAsync(new IdentityRole("Customer"));
            
            var adminUser = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            var adminResult = await userManager.CreateAsync(adminUser, "Admin123!");
            if (adminResult.Succeeded)
                await userManager.AddToRoleAsync(adminUser, "Admin");
            
            var customerUser = new ApplicationUser
            {
                UserName = "customer@example.com",
                Email = "customer@example.com",
                FirstName = "John",
                LastName = "Client",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            var customerResult = await userManager.CreateAsync(customerUser, "Customer123!");
            if (customerResult.Succeeded)
                await userManager.AddToRoleAsync(customerUser, "Customer");
            
            await context.SaveChangesAsync();
            
            var hotels = new List<Hotel>
            {
                new Hotel
                {
                    Name = "Grand Hotel & Spa",
                    City = "Kyiv",
                    Address = "Khreshchatyk St. 1",
                    Description = "Luxury 5-star hotel in the heart of Kyiv with spa center",
                    StarRating = 5,
                    PricePerNight = 6500,
                    MainImageUrl = "https://example.com/hotel1.jpg",
                    HasPool = true,
                    HasSpa = true,
                    HasRestaurant = true,
                    HasFreeWiFi = true,
                    HasParking = true,
                    IsActive = true,
                    Rooms = new List<Room>
                    {
                        new Room 
                        { 
                            RoomNumber = "101", 
                            RoomType = "Standard Double", 
                            Capacity = 2, 
                            PricePerNight = 2500,
                            Description = "Comfortable room with city view",
                            HasBreakfast = true,
                            HasAC = true,
                            HasTV = true,
                            IsAvailable = true
                        },
                        new Room 
                        { 
                            RoomNumber = "201", 
                            RoomType = "Deluxe Suite", 
                            Capacity = 3, 
                            PricePerNight = 4500,
                            Description = "Spacious suite with separate living area",
                            HasBreakfast = true,
                            HasAC = true,
                            HasTV = true,
                            HasMiniBar = true,
                            HasBalcony = true,
                            IsAvailable = true
                        },
                        new Room 
                        { 
                            RoomNumber = "301", 
                            RoomType = "Presidential Suite", 
                            Capacity = 4, 
                            PricePerNight = 8500,
                            Description = "Luxurious suite with panoramic views",
                            HasBreakfast = true,
                            HasAC = true,
                            HasTV = true,
                            HasMiniBar = true,
                            HasBalcony = true,
                            IsAvailable = false
                        }
                    }
                },
                new Hotel
                {
                    Name = "Historic Lviv Hotel",
                    City = "Lviv",
                    Address = "Rynok Square 15",
                    Description = "Charming hotel in historic Lviv center",
                    StarRating = 4,
                    PricePerNight = 3200,
                    MainImageUrl = "https://example.com/hotel2.jpg",
                    HasRestaurant = true,
                    HasFreeWiFi = true,
                    IsActive = true,
                    Rooms = new List<Room>
                    {
                        new Room 
                        { 
                            RoomNumber = "102", 
                            RoomType = "Standard Single", 
                            Capacity = 1, 
                            PricePerNight = 1800,
                            Description = "Cozy room for single travelers",
                            HasAC = true,
                            HasTV = true,
                            IsAvailable = true
                        },
                        new Room 
                        { 
                            RoomNumber = "202", 
                            RoomType = "Family Room", 
                            Capacity = 4, 
                            PricePerNight = 3500,
                            Description = "Perfect for families with children",
                            HasBreakfast = true,
                            HasAC = true,
                            HasTV = true,
                            IsAvailable = true
                        }
                    }
                },
                new Hotel
                {
                    Name = "Black Sea Resort",
                    City = "Odesa",
                    Address = "Primorsky Blvd. 25",
                    Description = "Beachfront hotel with sea views",
                    StarRating = 4,
                    PricePerNight = 4200,
                    MainImageUrl = "https://example.com/hotel3.jpg",
                    HasPool = true,
                    HasRestaurant = true,
                    HasFreeWiFi = true,
                    HasParking = true,
                    IsActive = true,
                    Rooms = new List<Room>
                    {
                        new Room 
                        { 
                            RoomNumber = "103", 
                            RoomType = "Sea View", 
                            Capacity = 2, 
                            PricePerNight = 3800,
                            Description = "Room with balcony overlooking the sea",
                            HasBreakfast = true,
                            HasAC = true,
                            HasTV = true,
                            HasBalcony = true,
                            IsAvailable = true
                        }
                    }
                }
            };
            
            context.Hotels.AddRange(hotels);
            await context.SaveChangesAsync();
            
            var bookings = new List<Booking>
            {
                new Booking
                {
                    UserId = customerUser.Id, 
                    RoomId = hotels[0].Rooms.First(r => r.RoomNumber == "101").Id, 
                    CheckInDate = DateTime.UtcNow.AddDays(7),
                    CheckOutDate = DateTime.UtcNow.AddDays(10),
                    GuestsCount = 2,
                    TotalPrice = 7500,
                    SpecialRequests = "We would like a late checkout if possible",
                    BookingDate = DateTime.UtcNow,
                    Status = BookingStatus.Confirmed
                },
                new Booking
                {
                    UserId = customerUser.Id,
                    RoomId = hotels[1].Rooms.First(r => r.RoomNumber == "202").Id, 
                    CheckInDate = DateTime.UtcNow.AddDays(14),
                    CheckOutDate = DateTime.UtcNow.AddDays(21),
                    GuestsCount = 3,
                    TotalPrice = 24500, 
                    BookingDate = DateTime.UtcNow,
                    Status = BookingStatus.Pending
                }
            };
            
            context.Bookings.AddRange(bookings);
            
            var reviews = new List<Review>
            {
                new Review
                {
                    UserId = customerUser.Id,
                    HotelId = hotels[0].Id,
                    Rating = 5,
                    Comment = "Excellent service and beautiful rooms!",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new Review
                {
                    UserId = customerUser.Id,
                    HotelId = hotels[1].Id,
                    Rating = 4,
                    Comment = "Great location, friendly staff",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                }
            };
            
            context.Reviews.AddRange(reviews);
            
            await context.SaveChangesAsync();
            
            Console.WriteLine($"Database seeded successfully!");
            Console.WriteLine($"Created: {hotels.Count} hotels, {hotels.Sum(h => h.Rooms.Count)} rooms");
            Console.WriteLine($"Created: 2 users, {bookings.Count} bookings, {reviews.Count} reviews");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding database: {ex.Message}");
        Console.WriteLine($"Full error: {ex}");
    }
}

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();