namespace HotelBookingApp.Application.DTOs
{
    public class CompleteBookingRequest
    {
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public List<GuestDetailDto> GuestDetails { get; set; } = new();
    }
    
    public class GuestDetailDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}