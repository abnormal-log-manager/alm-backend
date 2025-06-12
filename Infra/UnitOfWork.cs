using Application;
using Application.IRepos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly AppDbContext _context;
        public readonly IShortUrlRepo _shortUrlRepo;

        public UnitOfWork(AppDbContext context, IShortUrlRepo shortUrlRepo)
        {
            _context = context;
            _shortUrlRepo = shortUrlRepo;
        }
        public IShortUrlRepo ShortUrlRepo => _shortUrlRepo;
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
