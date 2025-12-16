using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotelBookingApp.Models;

namespace HotelBookingApp.Domain.Entities
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Room Type")]
        public string RoomType { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        [Display(Name = "Room Number")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, 10000)]
        [Display(Name = "Price Per Night")]
        [DataType(DataType.Currency)]
        public decimal PricePerNight { get; set; }

        [Required]
        [Range(1, 10)]
        [Display(Name = "Capacity")]
        public int Capacity { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Room Image URL")]
        public string? RoomImageUrl { get; set; }

        [Display(Name = "Has Breakfast")]
        public bool HasBreakfast { get; set; }

        [Display(Name = "Has Air Conditioning")]
        public bool HasAC { get; set; }

        [Display(Name = "Has TV")]
        public bool HasTV { get; set; }

        [Display(Name = "Has Mini Bar")]
        public bool HasMiniBar { get; set; }

        [Display(Name = "Has Balcony")]
        public bool HasBalcony { get; set; }

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;

        [Required]
        [Display(Name = "Hotel")]
        public int HotelId { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Updated")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("HotelId")]
        public virtual Hotel? Hotel { get; set; }

        public virtual ICollection<Booking>? Bookings { get; set; }
    }
}