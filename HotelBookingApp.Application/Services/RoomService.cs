using HotelBookingApp.Application.Common;
using HotelBookingApp.Application.DTOs;
using HotelBookingApp.Application.Interfaces;
using HotelBookingApp.Domain.Entities;
using HotelBookingApp.Models;
using CreateRoomRequest = HotelBookingApp.Application.Interfaces.CreateRoomRequest;

namespace HotelBookingApp.Application.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        
        public RoomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Response<IEnumerable<RoomDto>>> GetAvailableRoomsAsync(RoomSearchRequest request)
        {
            IEnumerable<Room> rooms;
            
            if (!string.IsNullOrEmpty(request.City) && request.CheckInDate.HasValue && 
                request.CheckOutDate.HasValue && request.Guests.HasValue)
            {
                rooms = (IEnumerable<Room>)await _unitOfWork.Rooms.SearchAvailableRoomsAsync(
                    request.City,
                    request.CheckInDate.Value,
                    request.CheckOutDate.Value,
                    request.Guests.Value);
            }
            else
            {
                rooms = await _unitOfWork.Rooms.GetAllAsync();
            }
            
            if (request.MaxPrice.HasValue)
            {
                rooms = rooms.Where(r => r.PricePerNight <= request.MaxPrice.Value);
            }
            
            var roomDtos = rooms.Select(r => MapToDto(r)).ToList();
            return Response<IEnumerable<RoomDto>>.Ok(roomDtos);
        }
        
        public async Task<Response<RoomDto>> GetRoomByIdAsync(int id)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);
            
            if (room == null)
            {
                return Response<RoomDto>.Fail("Room not found");
            }
            
            return Response<RoomDto>.Ok(MapToDto(room));
        }
        
        public async Task<Response<IEnumerable<RoomDto>>> GetRoomsByHotelIdAsync(int hotelId)
        {
            var rooms = await _unitOfWork.Rooms.GetRoomsByHotelIdAsync(hotelId);
            var roomDtos = rooms.Select(r => MapToDto(r)).ToList();
            
            return Response<IEnumerable<RoomDto>>.Ok(roomDtos);
        }
        
        public async Task<Response<RoomDto>> CreateRoomAsync(CreateRoomRequest request)
        {
            var hotel = await _unitOfWork.Hotels.GetByIdAsync(request.HotelId);
            
            if (hotel == null)
            {
                return Response<RoomDto>.Fail("Hotel not found");
            }
            
            var room = new Room
            {
                HotelId = request.HotelId,
                RoomNumber = request.RoomNumber,
                RoomType = request.RoomType,
                PricePerNight = request.PricePerNight,
                Capacity = request.Capacity,
                Description = request.Description,
                IsAvailable = true,
                CreatedAt = DateTime.UtcNow
            };
            
            await _unitOfWork.Rooms.AddAsync(room);
            await _unitOfWork.SaveChangesAsync();
            
            return Response<RoomDto>.Ok(MapToDto(room), "Room created successfully");
        }
        
        public async Task<Response<RoomDto>> UpdateRoomAsync(int id, CreateRoomRequest request)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);
            
            if (room == null)
            {
                return Response<RoomDto>.Fail("Room not found");
            }
            
            room.HotelId = request.HotelId;
            room.RoomNumber = request.RoomNumber;
            room.RoomType = request.RoomType;
            room.PricePerNight = request.PricePerNight;
            room.Capacity = request.Capacity;
            room.Description = request.Description;
            
            await _unitOfWork.Rooms.UpdateAsync(room);
            await _unitOfWork.SaveChangesAsync();
            
            return Response<RoomDto>.Ok(MapToDto(room), "Room updated successfully");
        }
        
        public async Task<Response<bool>> DeleteRoomAsync(int id)
        {
            var room = await _unitOfWork.Rooms.GetByIdAsync(id);
            
            if (room == null)
            {
                return Response<bool>.Fail("Room not found");
            }
            
            await _unitOfWork.Rooms.DeleteAsync(room);
            await _unitOfWork.SaveChangesAsync();
            
            return Response<bool>.Ok(true, "Room deleted successfully");
        }
        
        private RoomDto MapToDto(Room room)
        {
            return new RoomDto
            {
                Id = room.Id,
                HotelId = room.HotelId,
                HotelName = room.Hotel?.Name ?? string.Empty,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                PricePerNight = room.PricePerNight,
                Capacity = room.Capacity,
                Description = room.Description,
                IsAvailable = room.IsAvailable
            };
        }
    }
}