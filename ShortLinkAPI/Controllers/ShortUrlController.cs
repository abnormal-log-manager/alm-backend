using System.Runtime.CompilerServices;
using Application.IServices;
using Application.ViewModels.ShortUrl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ShortLinkAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShortUrlController : ControllerBase
    {
        private readonly IShortUrlService _service;
        public ShortUrlController(IShortUrlService service)
        {
            _service = service;
        }
        [HttpPost]
        public async Task<IActionResult> ShortenUrl(ShortUrlAddVM vm)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _service.ShortenUrlAsync(vm);
            return Ok(result);
        }
        [HttpGet]
        public async Task<IActionResult> ReadAllUrl(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest("Page and pageSize must be positive integers.");

            var (items, totalCount) = await _service.ReadAllAsync(page, pageSize);

            var response = new
            {
                TotalItems = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Data = items
            };

            return Ok(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> ReadUrl(int id)
        {
            var url = await _service.ReadAsync(id);
            if (url == null)
                return NotFound();
            return Ok(url);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUrl(int id)
        {
            await _service.DeleteAsync(id);
            return Ok("Url deleted");
        }
        [HttpDelete("softdel")]
        public async Task<IActionResult> SoftDeleteUrl(int id)
        {
            await _service.SoftDeleteAsync(id);
            return Ok("Url soft deleted");
        }
        [HttpGet("filter")]
        public async Task<IActionResult> Filter(
            int page = 1,
            int pageSize = 10,
            string? team = null,
            string? level = null,
            DateTime? createdDate = null,
            string? shortCode = null,
            string? sortBy = null,
            bool descending = false)
        {
            var (items, totalCount) = await _service.FilterAsync(page, pageSize, team, level, createdDate, sortBy, descending);
            return Ok(new
            {
                data = items,
                page,
                pageSize,
                totalItems = totalCount,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query is required");
            var result = await _service.SearchAsync(query);
            if (result == null)
                return NotFound("No matching URL found");
            return Ok(result);
        }
    }
}
