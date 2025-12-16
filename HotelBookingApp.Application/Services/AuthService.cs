using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelBookingApp.Application.Common;
using HotelBookingApp.Application.DTOs;
using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HotelBookingApp.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<Response<AuthResponse>> RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Response<AuthResponse>.Fail("Email and password are required");
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Response<AuthResponse>.Fail("Email already exists");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email.Trim().ToLower(),
                Email = request.Email.Trim().ToLower(),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return Response<AuthResponse>.Fail(
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(user, UserRoles.Customer);

            var token = GenerateJwtToken(user, UserRoles.Customer);

            return Response<AuthResponse>.Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email!,
                Role = UserRoles.Customer,
                FullName = $"{user.FirstName} {user.LastName}"
            }, "Registration successful");
        }

        public async Task<Response<AuthResponse>> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Response<AuthResponse>.Fail("Invalid email or password");
            }

            var isPasswordValid =
                await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isPasswordValid)
            {
                return Response<AuthResponse>.Fail("Invalid email or password");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? UserRoles.Customer;

            var token = GenerateJwtToken(user, role);

            return Response<AuthResponse>.Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email!,
                Role = role,
                FullName = $"{user.FirstName} {user.LastName}"
            }, "Login successful");
        }

        public Task<ApplicationUser?> GetUserByIdAsync(int userId)
        {
            throw new NotImplementedException();
        }

        private string GenerateJwtToken(ApplicationUser user, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Role, role),
                new Claim("FullName", $"{user.FirstName} {user.LastName}")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
