using Application.ViewModels.ShortUrl;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IServices
{
    public interface IShortUrlService
    {
        Task<ShortUrlVM> ShortenUrlAsync(ShortUrlAddVM vm);
        Task<IList<ShortUrlVM>> ShortenUrlBulkAsync(IEnumerable<ShortUrlAddVM> vms);
        Task<string?> GetOriginalUrlAsync(string shortCode);
        Task<(IList<ShortUrlVM> Items, int TotalCount)> ReadAllAsync(int page = 1, int pageSize = 10);
        Task<ShortUrlVM> ReadAsync(int id);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
        Task<(IList<ShortUrlVM> Items, int TotalCount)> FilterAsync(
    int page, int pageSize, string? team, string? level, DateTime? createdDate, string? sortBy, bool descending);
        Task<Dictionary<string, Dictionary<string, int>>> GetTeamStats(DateTime? from = null);
        Task<ShortUrlVM?> SearchAsync(string query);
    }
}
