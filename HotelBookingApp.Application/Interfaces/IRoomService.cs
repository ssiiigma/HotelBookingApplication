using HotelBookingApp.Application.Common;
using HotelBookingApp.Application.DTOs;

namespace HotelBookingApp.Application.Interfaces
{
    public interface IRoomService
    {
        Task<Response<IEnumerable<RoomDto>>> GetAvailableRoomsAsync(RoomSearchRequest request);
        Task<Response<RoomDto>> GetRoomByIdAsync(int id);
        Task<Response<IEnumerable<RoomDto>>> GetRoomsByHotelIdAsync(int hotelId);
        Task<Response<RoomDto>> CreateRoomAsync(CreateRoomRequest request);
        Task<Response<RoomDto>> UpdateRoomAsync(int id, CreateRoomRequest request);
        Task<Response<bool>> DeleteRoomAsync(int id);
    }
}