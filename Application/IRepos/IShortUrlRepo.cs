using Application.IRepos.Generics;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IRepos
{
    public interface IShortUrlRepo : IGenericRepo<ShortUrl>
    {
        Task<ShortUrl?> GetByShortCodeAsync(String shortCode);
        Task<ShortUrl?> GetByOriginalUrlAsync(String originalUrl);
        Task<(IList<ShortUrl> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize);
        Task<(IList<ShortUrl> Items, int TotalCount)> GetFilteredAsync(
    int page, int pageSize, string? team, string? level, DateTime? createdDate, string? sortBy, bool descending);
        Task<Dictionary<string, Dictionary<string, int>>> GetStatsPerTeamAsync(DateTime? from = null);
        Task<ShortUrl?> SearchAsync(String query);
        Task<bool> ExistsByShortCodeAsync(string shortCode);

    }
}
