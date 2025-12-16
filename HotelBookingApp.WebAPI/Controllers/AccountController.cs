using System.ComponentModel.DataAnnotations;
using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IIdentityService _identityService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IIdentityService identityService,
            ILogger<AccountController> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                Console.WriteLine($"Register attempt for: {request.Email}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse(false, "Invalid request data", ModelState));
                }

                var existingUser = await _identityService.FindUserByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return BadRequest(new ApiResponse(false, "User already exists"));
                }

                var user = new ApplicationUser
                {
                    UserName = request.FirstName,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true 
                };

                var result = await _identityService.CreateUserAsync(user, request.Password);

                if (result.Succeeded)
                {
                    await _identityService.AddUserToRoleAsync(user, "Customer");

                    await _identityService.SignInAsync(user, isPersistent: false);

                    var roles = await _identityService.GetUserRolesAsync(user);

                    return Ok(new ApiResponse(true, "Registration successful", new
                    {
                        user.Id,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        user.PhoneNumber,
                        roles,
                        isAdmin = roles.Contains("Admin")
                    }));
                }

                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new ApiResponse(false, "Registration failed", errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                Console.WriteLine($"Login attempt for: {request.Email}");

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse(false, "Invalid request data"));
                }

                var user = await _identityService.FindUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return Unauthorized(new ApiResponse(false, "Invalid email or password"));
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new ApiResponse(false, "Account is deactivated"));
                }

                var passwordValid = await _identityService.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    return Unauthorized(new ApiResponse(false, "Invalid email or password"));
                }

                await _identityService.SignInAsync(user, request.RememberMe);

                user.LastLogin = DateTime.UtcNow;
                await _identityService.UpdateUserAsync(user);

                var roles = await _identityService.GetUserRolesAsync(user);

                return Ok(new ApiResponse(true, "Login successful", new
                {
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    roles,
                    isAdmin = roles.Contains("Admin")
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse>> Logout()
        {
            try
            {
                await _identityService.SignOutAsync();
                return Ok(new ApiResponse(true, "Logout successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse>> GetProfile()
        {
            try
            {
                var user = await _identityService.GetCurrentUserAsync();
                if (user == null)
                {
                    return Unauthorized(new ApiResponse(false, "User not authenticated"));
                }

                var roles = await _identityService.GetUserRolesAsync(user);

                return Ok(new ApiResponse(true, "Profile retrieved", new
                {
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.PhoneNumber,
                    user.DateOfBirth,
                    user.CreatedAt,
                    user.LastLogin,
                    user.IsActive,
                    roles,
                    isAdmin = roles.Contains("Admin")
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpGet("check-auth")]
        public async Task<ActionResult<ApiResponse>> CheckAuth()
        {
            try
            {
                var user = await _identityService.GetCurrentUserAsync();
                if (user != null)
                {
                    var roles = await _identityService.GetUserRolesAsync(user);
                    
                    return Ok(new ApiResponse(true, "User authenticated", new
                    {
                        authenticated = true,
                        user = new
                        {
                            user.Id,
                            user.Email,
                            user.FirstName,
                            user.LastName,
                            roles,
                            isAdmin = roles.Contains("Admin")
                        }
                    }));
                }

                return Ok(new ApiResponse(true, "User not authenticated", new { authenticated = false }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking auth");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }
    }

    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public ApiResponse(bool success, string message, object? data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}