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
    var services = scope.ServiceProvider;
    
    try
    {
        var context = services.GetRequiredService<IApplicationDbContext>();
        
        var dbContext = context as DbContext;
        
        if (dbContext != null)
        {
            var hotelsExist = await dbContext.Set<Hotel>().AnyAsync();
            
            if (!hotelsExist)
            {
                Console.WriteLine("Заповнення бази даних тестовими данами...");
                
                var hotels = new List<Hotel>
                {
                    new Hotel
                    {
                        Name = "Grand Hotel & Spa",
                        City = "Kyiv",
                        Address = "Khreshchatyk St. 1",
                        Description = "Luxury 5-star hotel",
                        StarRating = 5,
                        PricePerNight = 6500,
                        HasPool = true,
                        HasSpa = true,
                        HasRestaurant = true,
                        HasFreeWiFi = true,
                        HasParking = true,
                        IsActive = true
                    },
                    new Hotel
                    {
                        Name = "Historic Lviv Hotel",
                        City = "Lviv",
                        Address = "Rynok Square 15",
                        Description = "Charming historic hotel",
                        StarRating = 4,
                        PricePerNight = 3200,
                        HasRestaurant = true,
                        HasFreeWiFi = true,
                        IsActive = true
                    }
                };
                
                await dbContext.Set<Hotel>().AddRangeAsync(hotels);
                await dbContext.SaveChangesAsync();
                
                var rooms = new List<Room>
                {
                    new Room 
                    { 
                        RoomNumber = "101", 
                        RoomType = "Standard", 
                        Capacity = 2, 
                        PricePerNight = 2500,
                        HotelId = hotels[0].Id,
                        IsAvailable = true
                    },
                    new Room 
                    { 
                        RoomNumber = "201", 
                        RoomType = "Deluxe", 
                        Capacity = 3, 
                        PricePerNight = 4500,
                        HotelId = hotels[0].Id,
                        IsAvailable = true
                    },
                    new Room 
                    { 
                        RoomNumber = "102", 
                        RoomType = "Family", 
                        Capacity = 4, 
                        PricePerNight = 3500,
                        HotelId = hotels[1].Id,
                        IsAvailable = true
                    }
                };
                
                await dbContext.Set<Room>().AddRangeAsync(rooms);
                await dbContext.SaveChangesAsync();
                
                Console.WriteLine($"Додано: {hotels.Count} готелів, {rooms.Count} кімнат");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Помилка заповнення БД: {ex.Message}");
    }
}

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();