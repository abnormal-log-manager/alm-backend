using Application.IRepos;
using Application.IRepos.Generics;
using Domain.Entities;
using Infra.Repos.Generics;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Application.Settings;

namespace Infra.Repos
{
    public class ShortUrlRepo : GenericRepo<ShortUrl>, IShortUrlRepo
    {
        private readonly AppDbContext _context;
        private readonly string _baseDomain;
        public ShortUrlRepo(AppDbContext context, IOptions<ShortUrlSettings> options) : base(context)
        {
            _context = context;
            _baseDomain = options.Value.BaseDomain;
        }
        // Lấy originalURL để check duplicate
        public async Task<ShortUrl?> GetByOriginalUrlAsync(string originalUrl)
        {
            return await _context.ShortUrls.
                FirstOrDefaultAsync(x => x.OriginalUrl == originalUrl);
        }
        // Lấy shortCode để tra originalUrl trong db
        public async Task<ShortUrl?> GetByShortCodeAsync(String shortCode)
        {
            return await _context.ShortUrls.
                FirstOrDefaultAsync(s => s.ShortenedUrl.EndsWith("/" + shortCode));
        }
        // Paging
        public async Task<(IList<ShortUrl> Items, int TotalCount)> GetPaginatedAsync(int page, int pageSize)
        {
            var query = _context.ShortUrls
                .Where(x => !x.IsDeleted) 
                .OrderBy(x => x.CreateDate); 

            var totalCount = await query.CountAsync(); // đếm tổng số bản ghi thỏa mản điều kiện trên
            var items = await query 
                .Skip((page - 1) * pageSize) // bỏ qua số lượng bản ghi tương ứng của các trang
                .Take(pageSize) // lấy đúng số lượng bản ghi của trang hiện tại 
                .ToListAsync(); 
            return (items, totalCount); // trả ds bản ghi cho trang hiện tại và tổng số bản ghi
        }
        public async Task<(IList<ShortUrl> Items, int TotalCount)> GetFilteredAsync(
    int page, int pageSize, string? team, string? level, DateTime? createdDate, string? sortBy, bool descending)
        {
            var query = _context.ShortUrls.AsQueryable();
            //thêm các điều kiện lọc
            if (!string.IsNullOrEmpty(team))
                query = query.Where(x => x.Team == team);

            if (!string.IsNullOrEmpty(level))
                query = query.Where(x => x.Level == level);

            if (createdDate.HasValue)
                query = query.Where(x => x.CreateDate.Date == createdDate.Value.Date);
            // sắp xếp
            query = sortBy switch
            {
                "id" => descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                "team" => descending ? query.OrderByDescending(x => x.Team) : query.OrderBy(x => x.Team),
                "level" => descending ? query.OrderByDescending(x => x.Level) : query.OrderBy(x => x.Level),
                "created" => descending ? query.OrderByDescending(x => x.CreateDate) : query.OrderBy(x => x.CreateDate),
                _ => query.OrderBy(x => x.Id)
            };

            var totalCount = await query.CountAsync(); // đếm tổng số kết quả sau khi ljc
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(); // paging

            return (items, totalCount);
        }
        public async Task<Dictionary<string, Dictionary<string, int>>> GetStatsPerTeamAsync(DateTime? from = null)
        {
            var query = _context.ShortUrls.Where(x => !x.IsDeleted);

            if (from.HasValue)
                query = query.Where(x => x.CreateDate >= from.Value);
            // nhóm theo team và level
            var result = await query
            .GroupBy(x => new { x.Team, x.Level })
            .Select(g => new
            {
                g.Key.Team,
                g.Key.Level,
                Count = g.Count()
            }).ToListAsync();
            // nhóm lại theo team, với mỗi team tạo Dictionany<Level, Count>
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
        public async Task<bool> ExistsByShortCodeAsync(string shortCode)
        {
            return await _context.ShortUrls
                .AnyAsync(s => s.ShortenedUrl.EndsWith($"/r/{shortCode}") && !s.IsDeleted);

        }
        public async Task<string> GenerateTitleFromUrl(string url)
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
                    return html.Substring(start + 7, end - (start + 7)).Trim();
                }
            }
            catch { }

            try
            {
                var uri = new Uri(url);
                return uri.Host.Replace("www.", "").Split('.').FirstOrDefault() ?? "Untitled";
            }
            catch
            {
                return "Untitled";
            }
        }

        public async Task<MemoryStream> ExportToExcelAsync()
        {
            var all = await _context.ShortUrls.AsNoTracking().ToListAsync();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("ShortUrls");

            var headers = new[] { "OriginalUrl", "ShortenedUrl", "Title", "Team", "Level", "CreateDate", "UpdateDate", "IsDeleted" };
            for (int i = 0; i < headers.Length; i++)
                sheet.Cells[1, i + 1].Value = headers[i];

            for (int i = 0;i < all.Count; i++)
            {
                var url = all[i];
                sheet.Cells[i + 1, 1].Value = url.OriginalUrl;
                sheet.Cells[i + 1, 2].Value = url.ShortenedUrl;
                sheet.Cells[i + 1, 3].Value = url.Title;
                sheet.Cells[i + 1, 4].Value = url.Team;
                sheet.Cells[i + 1, 5].Value = url.Level;
                sheet.Cells[i + 1, 6].Value = url.CreateDate.ToString("yyyy-MM-dd HH:mm:ss");
                sheet.Cells[i + 1, 7].Value = url.UpdateDate.ToString("yyyy-MM-dd HH:mm:ss");
                sheet.Cells[i + 1, 8].Value = url.IsDeleted;
            }
            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        public async Task<int> ImportFromExcelAsync(Stream stream)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null) return 0;

            int rowCount = sheet.Dimension.Rows;
            int imported = 0;

            for (int row = 2; row < rowCount; row++)
            {
                var originalUrl = sheet.Cells[row, 1].Text?.Trim();
                var title = sheet.Cells[row, 3].Text?.Trim();
                var team = sheet.Cells[row, 4].Text?.Trim();
                var level = sheet.Cells[row, 5].Text?.Trim();

                if (string.IsNullOrWhiteSpace(originalUrl)) continue;

                var exists = await _context.ShortUrls.FirstOrDefaultAsync(x => x.OriginalUrl == originalUrl);
                if (exists == null) continue;

                string shortCode;
                do
                {
                    shortCode = Guid.NewGuid().ToString("N")[..8];
                } while (await _context.ShortUrls.AnyAsync(x => x.ShortenedUrl.EndsWith($"/r/{shortCode}")));

                var entity = new ShortUrl
                {
                    OriginalUrl = originalUrl,
                    Title = string.IsNullOrWhiteSpace(title) ? await GenerateTitleFromUrl(originalUrl) : title,
                    Team = team,
                    Level = level,
                    ShortenedUrl = $"{_baseDomain.TrimEnd('/')}/r/{shortCode}",
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow,
                    IsDeleted = false
                };
                await _context.ShortUrls.AddAsync(entity);
                imported++;
            }
            await _context.SaveChangesAsync();
            return imported;
        }
    }
}