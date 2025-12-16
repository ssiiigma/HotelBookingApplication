using HotelBookingApp.Application.Common;
using HotelBookingApp.Application.DTOs;
using HotelBookingApp.Domain.Entities;

namespace HotelBookingApp.Application.Interfaces
{
    public interface IAuthService
    {
        Task<Response<AuthResponse>> RegisterAsync(RegisterRequest request);
        Task<Response<AuthResponse>> LoginAsync(LoginRequest request);
    }
}