using HotelBookingApp.Application.DTOs;
using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Domain.Entities;
using HotelBookingApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CreateHotelRequest = HotelBookingApp.Application.Interfaces.CreateHotelRequest;
using CreateRoomRequest = HotelBookingApp.Application.Interfaces.CreateRoomRequest;
using UpdateHotelRequest = HotelBookingApp.Application.Interfaces.UpdateHotelRequest;

namespace HotelBookingApp.Services
{
    public class HotelService : IHotelService
    {
        private readonly IApplicationDbContext _context; 
        private readonly ILogger<HotelService> _logger;

        public HotelService(IApplicationDbContext context, ILogger<HotelService> logger) 
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<HotelSearchResult>> SearchAsync(string city, DateTime checkIn, DateTime checkOut)
        {
            try
            {
                Console.WriteLine($"Search called: city={city}, checkIn={checkIn}, checkOut={checkOut}");
                
                var query = _context.Hotels
                    .Include(h => h.Rooms)
                    .Include(h => h.Reviews)
                    .Where(h => h.IsActive);

                if (!string.IsNullOrEmpty(city) && city != "null")
                {
                    query = query.Where(h => h.City.ToLower().Contains(city.ToLower()));
                }

                var hotels = await query.ToListAsync();
                Console.WriteLine($"Found {hotels.Count} hotels");

                var result = new List<HotelSearchResult>();

                foreach (var hotel in hotels)
                {
                    Console.WriteLine($"Processing hotel: {hotel.Name}");
                    
                    var availableRooms = new List<Room>();
                    
                    foreach (var room in hotel.Rooms.Where(r => r.IsAvailable))
                    {
                        var isBooked = await _context.Bookings.AnyAsync(b =>
                            b.RoomId == room.Id &&
                            b.Status != BookingStatus.Cancelled &&
                            ((checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                             (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                             (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)));

                        if (!isBooked)
                        {
                            availableRooms.Add(room);
                        }
                    }

                    Console.WriteLine($"Hotel {hotel.Name} has {availableRooms.Count} available rooms");

                    if (availableRooms.Any())
                    {
                        result.Add(new HotelSearchResult
                        {
                            Id = hotel.Id,
                            Name = hotel.Name,
                            City = hotel.City,
                            Address = hotel.Address,
                            Description = hotel.Description,
                            StarRating = hotel.StarRating,
                            MinPrice = availableRooms.Min(r => r.PricePerNight),
                            MaxPrice = availableRooms.Max(r => r.PricePerNight),
                            MainImageUrl = hotel.MainImageUrl ?? "/images/default-hotel.jpg",
                            HasPool = hotel.HasPool,
                            HasSpa = hotel.HasSpa,
                            HasRestaurant = hotel.HasRestaurant,
                            HasFreeWiFi = hotel.HasFreeWiFi,
                            HasParking = hotel.HasParking,
                            AvailableRooms = availableRooms.Count,
                            AverageRating = hotel.Reviews.Any() ? hotel.Reviews.Average(r => r.Rating) : 0,
                            ReviewCount = hotel.Reviews.Count
                        });
                    }
                }

                Console.WriteLine($"Returning {result.Count} results");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in SearchAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new List<HotelSearchResult>();
            }
        }

        public Task<IEnumerable<RoomAvailability>> GetAvailableRoomsAsync(int hotelId, DateTime checkIn, DateTime checkOut)
        {
            throw new NotImplementedException();
        }
        
        
        public async Task<ApiResponse> GetHotelByIdAsync(int hotelId)
        {
            try
            {
                var hotel = await _context.Hotels
                    .Include(h => h.Rooms)
                    .FirstOrDefaultAsync(h => h.Id == hotelId && h.IsActive);

                if (hotel == null)
                    return new ApiResponse(false, "Hotel not found");

                return new ApiResponse(true, "Hotel loaded successfully", hotel);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error loading hotel: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetRoomsByHotelIdAsync(int hotelId)
        {
            try
            {
                var rooms = await _context.Rooms
                    .Where(r => r.HotelId == hotelId && r.IsAvailable)
                    .ToListAsync();

                if (rooms.Count == 0)
                    return new ApiResponse(false, "No available rooms found");

                return new ApiResponse(true, "Rooms loaded successfully", rooms);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error loading rooms: {ex.Message}");
            }
        }
        
        public async Task<ServiceResponse<List<Room>>> GetRoomsByHotelIdAsync(int hotelId, DateTime? checkIn = null, DateTime? checkOut = null)
        {
            var response = new ServiceResponse<List<Room>>();

            var hotel = await _context.Hotels
                .Include(h => h.Rooms)
                .FirstOrDefaultAsync(h => h.Id == hotelId);

            if (hotel == null)
            {
                response.Success = false;
                response.Message = "Hotel not found";
                return response;
            }

            var rooms = hotel.Rooms?.ToList() ?? new List<Room>();

            if (checkIn.HasValue && checkOut.HasValue)
            {
                rooms = rooms.Where(r => !_context.Bookings.Any(b =>
                    b.RoomId == r.Id &&
                    ((checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                     (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                     (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate))
                )).ToList();
            }

            response.Data = rooms;
            return response;
        }

        
        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            return !await _context.Bookings.AnyAsync(b =>
                b.RoomId == roomId &&
                b.Status != BookingStatus.Cancelled &&
                ((checkIn >= b.CheckInDate && checkIn < b.CheckOutDate) ||
                 (checkOut > b.CheckInDate && checkOut <= b.CheckOutDate) ||
                 (checkIn <= b.CheckInDate && checkOut >= b.CheckOutDate)));
        }

        async public Task<ApiResponse> GetAllHotelsAsync()
        {
            try
            {
                // Завантажуємо готелі з кімнатами (без циклів)
                var hotels = await _context.Hotels
                    .Include(h => h.Rooms)
                    .Where(h => h.IsActive)
                    .ToListAsync();

                var hotelDtos = hotels.Select(h => new HotelDto
                {
                    Id = h.Id,
                    Name = h.Name,
                    City = h.City,
                    Description = h.Description,
                    MinPrice = h.Rooms.Any() ? h.Rooms.Min(r => r.PricePerNight) : 0,
                    MaxPrice = h.Rooms.Any() ? h.Rooms.Max(r => r.PricePerNight) : 0,
                    AvailableRooms = h.Rooms.Count(r => r.IsAvailable)
                }).ToList();

                return new ApiResponse(true, "Hotels loaded", hotelDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading hotels");
                return new ApiResponse(false, "Error loading hotels");
            }

        }

        

        public Task<ServiceResponse<AdminHotelResponse>> CreateHotelAsync(CreateHotelRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<AdminHotelResponse>> UpdateHotelAsync(UpdateHotelRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<bool>> DeleteHotelAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<IEnumerable<AdminRoomResponse>>> GetAllRoomsAsync(int? hotelId = null)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<AdminRoomResponse>> GetRoomByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<AdminRoomResponse>> CreateRoomAsync(CreateRoomRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<AdminRoomResponse>> UpdateRoomAsync(UpdateRoomRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<bool>> DeleteRoomAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponse<bool>> UpdateRoomAvailabilityAsync(int id, bool isAvailable)
        {
            throw new NotImplementedException();
        }

        public async Task<HotelDetails> GetHotelDetailsAsync(int hotelId)
        {
            try
            {
                var hotel = await _context.Hotels
                    .Include(h => h.Rooms)
                    .Include(h => h.Reviews)
                    .ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(h => h.Id == hotelId && h.IsActive);

                if (hotel == null)
                    throw new ArgumentException($"Hotel with ID {hotelId} not found");

                return new HotelDetails
                {
                    Id = hotel.Id,
                    Name = hotel.Name,
                    City = hotel.City,
                    Address = hotel.Address,
                    Description = hotel.Description,
                    StarRating = hotel.StarRating,
                    MainImageUrl = hotel.MainImageUrl,
                    GalleryImages = hotel.GalleryImages,
                    Amenities = new HotelAmenities
                    {
                        HasPool = hotel.HasPool,
                        HasSpa = hotel.HasSpa,
                        HasRestaurant = hotel.HasRestaurant,
                        HasFreeWiFi = hotel.HasFreeWiFi,
                        HasParking = hotel.HasParking
                    },
                    Rooms = hotel.Rooms.Select(r => new RoomInfo
                    {
                        Id = r.Id,
                        RoomNumber = r.RoomNumber,
                        RoomType = r.RoomType,
                        Description = r.Description,
                        Capacity = r.Capacity,
                        PricePerNight = r.PricePerNight,
                        RoomImageUrl = r.RoomImageUrl,
                        Amenities = new RoomAmenities
                        {
                            HasBreakfast = r.HasBreakfast,
                            HasAC = r.HasAC,
                            HasTV = r.HasTV,
                            HasMiniBar = r.HasMiniBar,
                            HasBalcony = r.HasBalcony
                        },
                        IsAvailable = r.IsAvailable
                    }).ToList(),
                    Reviews = hotel.Reviews.Select(r => new ReviewInfo
                    {
                        Id = r.Id,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt,
                        UserName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : "Anonymous"
                    }).ToList(),
                    AverageRating = hotel.Reviews.Any() ? hotel.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = hotel.Reviews.Count
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetHotelDetailsAsync: {ex.Message}");
                throw;
            }
        }
    }
}