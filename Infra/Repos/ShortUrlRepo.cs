using Application.IRepos;
using Application.IRepos.Generics;
using Domain.Entities;
using Infra.Repos.Generics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Repos
{
    public class ShortUrlRepo : GenericRepo<ShortUrl>, IShortUrlRepo
    {
        private readonly AppDbContext _context;
        public ShortUrlRepo(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ShortUrl?> GetByOriginalUrlAsync(string originalUrl)
        {
            return await _context.ShortUrls.
                FirstOrDefaultAsync(x => x.OriginalUrl == originalUrl);
        }

        public async Task<ShortUrl?> GetByShortCodeAsync(String shortCode)
        {
            return await _context.ShortUrls.
                FirstOrDefaultAsync(s => s.ShortenedUrl.EndsWith("/" + shortCode));
        }
        public async Task<(IList<ShortUrl> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            var query = _context.ShortUrls
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.CreateDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, totalCount);
        }
        public async Task<(IList<ShortUrl> Items, int TotalCount)> GetFilteredAsync(
    int page, int pageSize, string? team, string? level, DateTime? createdDate, string? sortBy, bool descending)
        {
            var query = _context.ShortUrls.AsQueryable();

            if (!string.IsNullOrEmpty(team))
                query = query.Where(x => x.Team == team);

            if (!string.IsNullOrEmpty(level))
                query = query.Where(x => x.Level == level);

            if (createdDate.HasValue)
                query = query.Where(x => x.CreateDate.Date == createdDate.Value.Date);

            query = sortBy switch
            {
                "id" => descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                "team" => descending ? query.OrderByDescending(x => x.Team) : query.OrderBy(x => x.Team),
                "level" => descending ? query.OrderByDescending(x => x.Level) : query.OrderBy(x => x.Level),
                "created" => descending ? query.OrderByDescending(x => x.CreateDate) : query.OrderBy(x => x.CreateDate),
                _ => query.OrderBy(x => x.Id)
            };

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }
        public async Task<Dictionary<string, Dictionary<string, int>>> GetStatsPerTeamAsync(DateTime? from = null)
        {
            var query = _context.ShortUrls.Where(x => !x.IsDeleted);

            if (from.HasValue)
                query = query.Where(x => x.CreateDate >= from.Value);

            var result = await query
            .GroupBy(x => new { x.Team, x.Level })
            .Select(g => new
            {
                g.Key.Team,
                g.Key.Level,
                Count = g.Count()
            }).ToListAsync();

            return result
            .GroupBy(x => x.Team)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(x => x.Level, x => x.Count)
            );
        }
        public async Task<ShortUrl?> SearchAsync(String query)
        {
            string? shortCode = null;
            if (Uri.TryCreate(query, UriKind.Absolute, out var uri))
            {
                shortCode = uri.Segments.LastOrDefault()?.TrimEnd('/');
            } 
            else
            {
                shortCode = query;
            }
            return await _context.ShortUrls.FirstOrDefaultAsync(x =>
            x.OriginalUrl == query ||
            x.ShortenedUrl == query ||
            x.ShortenedUrl.EndsWith("/" + shortCode));
        }
    }
}
