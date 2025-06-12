using Application.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ShortLinkAPI.Controllers
{
    [Route("api/r")]
    [ApiController]
    public class RedirectController : ControllerBase
    {
        private readonly IShortUrlService _service;
        public RedirectController(IShortUrlService service)
        {
            _service = service;
        }
        [HttpGet("{shortCode}")]
        public async Task<IActionResult> RedirectUrl(string shortCode)
        {
            var originalUrl = await _service.GetOriginalUrlAsync(shortCode);
            if (originalUrl == null)
            {
                return NotFound("Shortened URL not found");
            }
            return Redirect(originalUrl);
        }
    }
}
