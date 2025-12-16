using HotelBookingApp.Application.DTOs;
using HotelBookingApp.Domain.Entities;

namespace HotelBookingApp.Application.Interfaces
{
    public interface IHotelService
    {
        // Пошук та отримання інформації
        Task<IEnumerable<HotelSearchResult>> SearchAsync(string city, DateTime checkIn, DateTime checkOut);
        Task<HotelDetails> GetHotelDetailsAsync(int hotelId);
        Task<IEnumerable<RoomAvailability>> GetAvailableRoomsAsync(int hotelId, DateTime checkIn, DateTime checkOut);
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut);
        
        // Адмін функції для готелів
        Task<ApiResponse> GetAllHotelsAsync();
        Task<ApiResponse> GetHotelByIdAsync(int id);
        Task<ServiceResponse<AdminHotelResponse>> CreateHotelAsync(CreateHotelRequest request);
        Task<ServiceResponse<AdminHotelResponse>> UpdateHotelAsync(UpdateHotelRequest request);
        Task<ServiceResponse<bool>> DeleteHotelAsync(int id);
        
        // Адмін функції для кімнат
        Task<ServiceResponse<IEnumerable<AdminRoomResponse>>> GetAllRoomsAsync(int? hotelId = null);
        Task<ServiceResponse<AdminRoomResponse>> GetRoomByIdAsync(int id);
        Task<ServiceResponse<AdminRoomResponse>> CreateRoomAsync(CreateRoomRequest request);
        Task<ServiceResponse<AdminRoomResponse>> UpdateRoomAsync(UpdateRoomRequest request);
        Task<ServiceResponse<bool>> DeleteRoomAsync(int id);
        Task<ServiceResponse<bool>> UpdateRoomAvailabilityAsync(int id, bool isAvailable);
    }

    // ============ МОДЕЛІ ДЛЯ ПОШУКУ ============
    public class HotelSearchResult
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int StarRating { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public string? MainImageUrl { get; set; }
        public bool HasPool { get; set; }
        public bool HasSpa { get; set; }
        public bool HasRestaurant { get; set; }
        public bool HasFreeWiFi { get; set; }
        public bool HasParking { get; set; }
        public int AvailableRooms { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class HotelDetails
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int StarRating { get; set; }
        public string? MainImageUrl { get; set; }
        public string? GalleryImages { get; set; }
        public HotelAmenities Amenities { get; set; } = new HotelAmenities();
        public IEnumerable<RoomInfo> Rooms { get; set; } = new List<RoomInfo>();
        public IEnumerable<ReviewInfo> Reviews { get; set; } = new List<ReviewInfo>();
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class HotelAmenities
    {
        public bool HasPool { get; set; }
        public bool HasSpa { get; set; }
        public bool HasRestaurant { get; set; }
        public bool HasFreeWiFi { get; set; }
        public bool HasParking { get; set; }
    }

    public class RoomInfo
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public string? RoomImageUrl { get; set; }
        public RoomAmenities Amenities { get; set; } = new RoomAmenities();
        public bool IsAvailable { get; set; }
    }

    public class RoomAmenities
    {
        public bool HasBreakfast { get; set; }
        public bool HasAC { get; set; }
        public bool HasTV { get; set; }
        public bool HasMiniBar { get; set; }
        public bool HasBalcony { get; set; }
    }

    public class ReviewInfo
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
    }

    public class RoomAvailability
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public string? RoomImageUrl { get; set; }
        public RoomAmenities Amenities { get; set; } = new RoomAmenities();
        public bool IsAvailable { get; set; }
        public decimal TotalPrice { get; set; }
        public int Nights { get; set; }
    }

    // ============ АДМІН МОДЕЛІ ============
    public class AdminHotelResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int StarRating { get; set; }
        public decimal PricePerNight { get; set; }
        public string? MainImageUrl { get; set; }
        public string? GalleryImages { get; set; }
        public bool HasPool { get; set; }
        public bool HasSpa { get; set; }
        public bool HasRestaurant { get; set; }
        public bool HasFreeWiFi { get; set; }
        public bool HasParking { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RoomCount { get; set; }
        public int BookingCount { get; set; }
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }
    }

    public class CreateHotelRequest
    {
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int StarRating { get; set; }
        public decimal PricePerNight { get; set; }
        public string? MainImageUrl { get; set; }
        public string? GalleryImages { get; set; }
        public bool HasPool { get; set; }
        public bool HasSpa { get; set; }
        public bool HasRestaurant { get; set; }
        public bool HasFreeWiFi { get; set; }
        public bool HasParking { get; set; }
    }

    public class UpdateHotelRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int StarRating { get; set; }
        public decimal PricePerNight { get; set; }
        public string? MainImageUrl { get; set; }
        public string? GalleryImages { get; set; }
        public bool HasPool { get; set; }
        public bool HasSpa { get; set; }
        public bool HasRestaurant { get; set; }
        public bool HasFreeWiFi { get; set; }
        public bool HasParking { get; set; }
        public bool IsActive { get; set; }
    }

    public class AdminRoomResponse
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public string? RoomImageUrl { get; set; }
        public bool HasBreakfast { get; set; }
        public bool HasAC { get; set; }
        public bool HasTV { get; set; }
        public bool HasMiniBar { get; set; }
        public bool HasBalcony { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int HotelId { get; set; }
        public string HotelName { get; set; } = string.Empty;
        public string HotelCity { get; set; } = string.Empty;
        public int BookingCount { get; set; }
    }

    public class CreateRoomRequest
    {
        public int HotelId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public string? RoomImageUrl { get; set; }
        public bool HasBreakfast { get; set; }
        public bool HasAC { get; set; }
        public bool HasTV { get; set; }
        public bool HasMiniBar { get; set; }
        public bool HasBalcony { get; set; }
    }

    public class UpdateRoomRequest
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public string? Description { get; set; }
        public string? RoomImageUrl { get; set; }
        public bool HasBreakfast { get; set; }
        public bool HasAC { get; set; }
        public bool HasTV { get; set; }
        public bool HasMiniBar { get; set; }
        public bool HasBalcony { get; set; }
        public bool IsAvailable { get; set; }
    }

    // ============ УНІВЕРСАЛЬНА ВІДПОВІДЬ ============
    public class ServiceResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ServiceResponse<T> Ok(T data, string message = "Success")
        {
            return new ServiceResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ServiceResponse<T> Fail(string message, List<string>? errors = null)
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}