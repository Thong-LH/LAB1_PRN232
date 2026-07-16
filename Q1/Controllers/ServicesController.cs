using Q1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Q1.Controllers
{
    [ApiController]
    [Route("api/services")]
    public class ServicesController : ControllerBase
    {
        private readonly HotelContext _context;

        public ServicesController(HotelContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetServices()
        {
            var services = await _context.Services
                .Select(s => new
                {
                    s.ServiceId,
                    s.ServiceName
                })
                .ToListAsync();

            return Ok(services);
        }
    }
}
