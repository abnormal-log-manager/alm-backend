using Application.IRepos;
using Application.IServices;
using Application.Settings;
using Application.Extensions;
using Application.ViewModels.ShortUrl;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class ShortUrlService : IShortUrlService
    {
        private readonly IShortUrlRepo _repo;
        private readonly IUnitOfWork _unit;
        private readonly IMapper _mapper;
        private readonly String _baseDomain;
        public ShortUrlService(IShortUrlRepo repo, IUnitOfWork unit, IMapper mapper, IOptions<ShortUrlSettings> options)
        {
            _repo = repo;
            _unit = unit;
            _mapper = mapper;
            _baseDomain = options.Value.BaseDomain;
        }

        public async Task DeleteAsync(int id)
        {
            var item = await _unit.ShortUrlRepo.ReadAsync(id);

            _unit.ShortUrlRepo.Delete(item);
            await _unit.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id)
        {
            var record = await _unit.ShortUrlRepo.ReadAsync(id);
            _unit.ShortUrlRepo.SoftDelete(record);
            await _unit.SaveChangesAsync();
        }
        public async Task<string?> GetOriginalUrlAsync(string shortCode)
        {
            var shortUrl = await _repo.GetByShortCodeAsync(shortCode);
            return shortUrl?.OriginalUrl;
        }

        public async Task<(IList<ShortUrlVM> Items, int TotalCount)> ReadAllAsync(int page = 1, int pageSize = 10)
        {
            var (items, totalCount) = await _repo.GetPaginatedAsync(page, pageSize);
            var result = _mapper.Map<IList<ShortUrlVM>>(items);
            return (result, totalCount);
        }

        public async Task<ShortUrlVM> ReadAsync(int id)
        {
            var item = await _unit.ShortUrlRepo.ReadAsync(id);
            var result = _mapper.Map<ShortUrlVM>(item);
            return result;
        }
        private async Task<string> GenerateTitleFromUrl(string url)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);

                var html = await client.GetStringAsync(url);
                var start = html.IndexOf("<title>", StringComparison.OrdinalIgnoreCase);
                var end = html.IndexOf("</title>", StringComparison.OrdinalIgnoreCase);

                if (start != -1 && end != -1 && end > start)
                {
                    var title = html.Substring(start + 7, end - (start + 7)).Trim();
                    if (!string.IsNullOrWhiteSpace(title))
                        return title;
                }
            }
            catch
            {
                // swallow and fallback
            }

            try
            {
                var uri = new Uri(url);
                return uri.Host.Replace("www.", "").Split('.').FirstOrDefault()?.CapitalizeFirst() ?? "Untitled Link";
            }
            catch
            {
                return "Untitled Link";
            }
        }

        public async Task<ShortUrlVM> ShortenUrlAsync(ShortUrlAddVM vm)
        {
            var existing = await _repo.GetByOriginalUrlAsync(vm.OriginalUrl);
            if (existing != null)
            {
                throw new InvalidOperationException("This URL already exists.");
            }
            var entity = _mapper.Map<ShortUrl>(vm);
            if (string.IsNullOrWhiteSpace(entity.Title))
            {
                entity.Title = await GenerateTitleFromUrl(entity.OriginalUrl);
            }
            var shortCode = GeneratedShortCode();
            entity.ShortenedUrl = $"{_baseDomain.TrimEnd('/')}/r/{shortCode}";
            await _repo.CreateAsync(entity);
            await _unit.SaveChangesAsync();
            return _mapper.Map<ShortUrlVM>(entity);
        }
        public async Task<IList<ShortUrlVM>> ShortenUrlBulkAsync(IEnumerable<ShortUrlAddVM> vms)
        {
            var results = new List<ShortUrlVM>();

            foreach (var vm in vms)
            {
                var existing = await _repo.GetByOriginalUrlAsync(vm.OriginalUrl);
                if (existing != null)
                {
                    results.Add(_mapper.Map<ShortUrlVM>(existing));
                    continue;
                }
                var entity = _mapper.Map<ShortUrl>(vm);
                if (string.IsNullOrWhiteSpace(entity.Title))
                {
                    entity.Title = await GenerateTitleFromUrl(entity.OriginalUrl);
                }
                var shortCode = GeneratedShortCode();
                entity.ShortenedUrl = $"{_baseDomain.TrimEnd('/')}/r/{shortCode}";
                await _repo.CreateAsync(entity);
                results.Add(_mapper.Map<ShortUrlVM>(entity));
            }
            await _unit.SaveChangesAsync();
            return results;
        }
        private String GeneratedShortCode()
        {
            return Guid.NewGuid().ToString("N")[..8];
        }
        public async Task<(IList<ShortUrlVM> Items, int TotalCount)> FilterAsync(
    int page, int pageSize, string? team, string? level, DateTime? createdDate, string? sortBy, bool descending)
        {
            var (items, totalCount) = await _repo.GetFilteredAsync(page, pageSize, team, level, createdDate, sortBy, descending);
            return (_mapper.Map<IList<ShortUrlVM>>(items), totalCount);
        }
        public async Task<Dictionary<string, Dictionary<string, int>>> GetTeamStats(DateTime? from = null)
        {
            return await _repo.GetStatsPerTeamAsync(from);
        }
        public async Task<ShortUrlVM?> SearchAsync(string query)
        {
            var result = await _repo.SearchAsync(query);
            return result == null ? null : _mapper.Map<ShortUrlVM>(result);
        }
        public async Task<ShortUrlVM?> UpdateAsync(int id, string? title, string? team, string? level)
        {
            var entity = await _unit.ShortUrlRepo.ReadAsync(id);
            if (entity == null || entity.IsDeleted)
                return null;
            entity.Title = title ?? entity.Title;
            entity.Team = team ?? entity.Team;
            entity.Level = level ?? entity.Level;
            entity.UpdateDate = DateTime.Now;
            await _unit.SaveChangesAsync();
            return _mapper.Map<ShortUrlVM>(entity);
        }
    }
}
