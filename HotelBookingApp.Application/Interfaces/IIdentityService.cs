using HotelBookingApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace HotelBookingApp.Application.Interfaces
{
    public interface IIdentityService
    {
        Task<ApplicationUser> FindUserByEmailAsync(string email);
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
        Task<IdentityResult> AddUserToRoleAsync(ApplicationUser user, string role);
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task SignInAsync(ApplicationUser user, bool isPersistent);
        Task SignOutAsync();
        Task<ApplicationUser> GetCurrentUserAsync();
    }
}