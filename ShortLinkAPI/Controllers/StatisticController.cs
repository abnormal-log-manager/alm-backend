using Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ShortLinkAPI.Controllers
{
    [Route("api/stats")]
    [ApiController]

    public class StatisticController : ControllerBase
    {
        private readonly IShortUrlService _service;
        public StatisticController(IShortUrlService service)
        {
            _service = service;
        }
        [HttpGet("per-team")]
        public async Task<IActionResult> GetTeamStats([FromQuery] int? days = null)
        {
            DateTime? from = days.HasValue ? DateTime.UtcNow.AddDays(-days.Value) : null;
            var result = await _service.GetTeamStats(from);
            return Ok(result);
        }
    }
}