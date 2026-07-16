using Q1.DTOs;
using Q1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Q1.Controllers
{
    [ApiController]
    [Route("api/roomtypes")]
    public class RoomTypesController : ControllerBase
    {
        private readonly HotelContext _context;

        public RoomTypesController(HotelContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomTypeDTO>>> GetRoomTypes()
        {
            var roomTypes = await _context.RoomTypes
                .Select(rt => new RoomTypeDTO
                {
                    RoomTypeId = rt.RoomTypeId,
                    TypeName = rt.TypeName,
                    BasePrice = rt.BasePrice,
                    ServiceCount = rt.RoomTypeServices.Count
                })
                .ToListAsync();

            return Ok(roomTypes);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<SearchRoomTypeDTO>>> SearchRoomTypes([FromQuery] int serviceId, [FromQuery] string? priceRange)
        {
            var query = _context.RoomTypes.AsQueryable();

            if (serviceId > 0)
            {
                query = query.Where(rt => rt.RoomTypeServices.Any(rts => rts.ServiceId == serviceId));
            }

            if (!string.IsNullOrEmpty(priceRange))
            {
                if (priceRange.Contains("< 1M"))
                {
                    query = query.Where(rt => rt.BasePrice < 1000000);
                }
                else if (priceRange.Contains(">= 1M"))
                {
                    query = query.Where(rt => rt.BasePrice >= 1000000);
                }
            }

            var roomTypes = await query
                .Select(rt => new SearchRoomTypeDTO
                {
                    RoomTypeId = rt.RoomTypeId,
                    TypeName = rt.TypeName,
                    BasePrice = rt.BasePrice,
                    Services = rt.RoomTypeServices.Select(rts => new SearchServiceDTO
                    {
                        ServiceId = rts.ServiceId,
                        ServiceName = rts.Service!.ServiceName
                    }).ToList()
                })
                .ToListAsync();

            return Ok(roomTypes);
        }

        [HttpGet("{roomTypeId}")]
        public async Task<ActionResult<RoomTypeDetailDTO>> GetRoomTypeDetail(int roomTypeId)
        {
            var roomType = await _context.RoomTypes
                .Where(rt => rt.RoomTypeId == roomTypeId)
                .Select(rt => new RoomTypeDetailDTO
                {
                    RoomTypeId = rt.RoomTypeId,
                    TypeName = rt.TypeName,
                    BasePrice = rt.BasePrice,
                    Services = rt.RoomTypeServices.Select(rts => new SearchServiceDTO
                    {
                        ServiceId = rts.ServiceId,
                        ServiceName = rts.Service!.ServiceName
                    }).ToList(),
                    Rooms = rt.Rooms.Select(r => new RoomTypeDetailRoomDTO
                    {
                        RoomId = r.RoomId,
                        RoomNumber = r.RoomNumber,
                        Status = r.Status
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (roomType == null)
            {
                return NotFound();
            }

            return Ok(roomType);
        }
    }
}
