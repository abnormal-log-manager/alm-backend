using Application.IRepos;
using Application.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public interface IUnitOfWork
    {
        IShortUrlRepo ShortUrlRepo { get; }
        public Task<int> SaveChangesAsync();
    }
}
