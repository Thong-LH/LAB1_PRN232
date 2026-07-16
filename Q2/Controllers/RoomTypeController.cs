using Microsoft.AspNetCore.Mvc;
using Q2.Models;
using System.Net.Http.Json;

namespace Q2.Controllers
{
    public class RoomTypeController : Controller
    {
        private readonly HttpClient _httpClient;

        public RoomTypeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("RoomType")]
        public async Task<IActionResult> Index([FromQuery] int? serviceId, [FromQuery] string? priceRange)
        {
            int selectedServiceId = serviceId ?? 0;
            string selectedPriceRange = priceRange ?? "All prices";

            // Load services for dropdown
            string servicesUrl = Utilities.GetAbsoluteUrl("/api/services");
            var services = await _httpClient.GetFromJsonAsync<List<ServiceVM>>(servicesUrl) ?? new List<ServiceVM>();

            // Load filtered room types
            string searchUrl = Utilities.GetAbsoluteUrl("/api/roomtypes/search?serviceId=" + selectedServiceId + "&priceRange=" + selectedPriceRange);
            var roomTypes = await _httpClient.GetFromJsonAsync<List<RoomTypeSearchVM>>(searchUrl) ?? new List<RoomTypeSearchVM>();

            var model = new RoomTypeIndexVM
            {
                SelectedServiceId = selectedServiceId,
                SelectedPriceRange = selectedPriceRange,
                Services = services,
                RoomTypes = roomTypes
            };

            return View(model);
        }

        [HttpGet("RoomType/Analyze/{id}")]
        public async Task<IActionResult> Analyze(int id)
        {
            string detailUrl = Utilities.GetAbsoluteUrl("/api/roomtypes/" + id);
            try
            {
                var detail = await _httpClient.GetFromJsonAsync<RoomTypeDetailVM>(detailUrl);
                if (detail == null)
                {
                    return NotFound();
                }
                return View(detail);
            }
            catch (HttpRequestException)
            {
                return NotFound();
            }
        }
    }
}
