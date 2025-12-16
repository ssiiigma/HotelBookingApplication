using System.ComponentModel.DataAnnotations;
using HotelBookingApp.Models;

namespace HotelBookingApp.Domain.Entities
{
    public class Hotel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Hotel Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(1, 5)]
        [Display(Name = "Star Rating")]
        public int StarRating { get; set; }

        [Display(Name = "Price Per Night (from)")]
        [DataType(DataType.Currency)]
        public decimal PricePerNight { get; set; }

        [Display(Name = "Main Image URL")]
        public string? MainImageUrl { get; set; }

        [Display(Name = "Gallery Images")]
        public string? GalleryImages { get; set; } 

        [Display(Name = "Has Swimming Pool")]
        public bool HasPool { get; set; }

        [Display(Name = "Has Spa")]
        public bool HasSpa { get; set; }

        [Display(Name = "Has Restaurant")]
        public bool HasRestaurant { get; set; }

        [Display(Name = "Has Free WiFi")]
        public bool HasFreeWiFi { get; set; }

        [Display(Name = "Has Parking")]
        public bool HasParking { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Room>? Rooms { get; set; }
        public virtual ICollection<Review>? Reviews { get; set; }
    }
}