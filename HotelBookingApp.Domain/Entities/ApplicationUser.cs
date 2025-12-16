using System.ComponentModel.DataAnnotations;
using HotelBookingApp.Models;
using Microsoft.AspNetCore.Identity;

namespace HotelBookingApp.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public string? ProfileImage { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLogin { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Booking>? Bookings { get; set; }
        public ICollection<Review>? Reviews { get; set; }

    }
    
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";
    }
}