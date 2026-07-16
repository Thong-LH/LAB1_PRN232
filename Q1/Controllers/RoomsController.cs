using Q1.DTOs;
using Q1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Q1.Controllers
{
    [ApiController]
    [Route("api")]
    public class RoomsController : ControllerBase
    {
        private readonly HotelContext _context;

        public RoomsController(HotelContext context)
        {
            _context = context;
        }

        [HttpGet("filter-rooms")]
        public async Task<ActionResult<IEnumerable<FilteredRoomDTO>>> FilterRooms([FromQuery] string? status, [FromQuery] decimal? minPrice)
        {
            if (minPrice.HasValue && minPrice.Value < 0)
            {
                return BadRequest();
            }

            var query = _context.Rooms.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status.ToLower() == status.ToLower());
            }

            if (minPrice.HasValue)
            {
                query = query.Where(r => r.RoomType!.BasePrice >= minPrice.Value);
            }

            query = query.OrderByDescending(r => r.RoomType!.BasePrice);

            var rooms = await query
                .Select(r => new
                {
                    r.RoomId,
                    r.RoomNumber,
                    r.Status,
                    BasePrice = r.RoomType!.BasePrice,
                    TotalNights = r.BookingDetails.Sum(bd => bd.NightCount),
                    CurrentGuest = r.BookingDetails
                        .OrderByDescending(bd => bd.Booking!.CheckInDate)
                        .Select(bd => bd.Booking!.Guest!.FullName)
                        .FirstOrDefault(),
                    ServiceNames = r.RoomType!.RoomTypeServices.Select(rts => rts.Service!.ServiceName).ToList()
                })
                .ToListAsync();

            var result = rooms.Select(r => new FilteredRoomDTO
            {
                RoomId = r.RoomId,
                RoomNumber = r.RoomNumber,
                Status = r.Status,
                BasePrice = r.BasePrice,
                TotalNights = r.TotalNights,
                CurrentGuest = r.CurrentGuest ?? "No Guest",
                ServiceList = r.ServiceNames
            }).ToList();

            return Ok(result);
        }

        public class CreateRoomRequest
        {
            public string RoomNumber { get; set; } = null!;
            public int RoomTypeId { get; set; }
            public string Status { get; set; } = null!;
        }

        [HttpPost("rooms")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
        {
            if (await _context.Rooms.AnyAsync(r => r.RoomNumber == request.RoomNumber))
            {
                return BadRequest("Room number already exists");
            }

            var room = new Room
            {
                RoomNumber = request.RoomNumber,
                RoomTypeId = request.RoomTypeId,
                Status = request.Status,
                RoomType = null,
                BookingDetails = new List<BookingDetail>()
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return StatusCode(201, room);
        }

        [HttpDelete("rooms/{roomId}")]
        public async Task<IActionResult> DeleteRoom(int roomId)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                return NotFound();
            }

            if (await _context.BookingDetails.AnyAsync(bd => bd.RoomId == roomId))
            {
                return BadRequest("Cannot delete room with active booking history");
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
