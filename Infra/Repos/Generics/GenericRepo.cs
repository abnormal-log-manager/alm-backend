using Application.IRepos.Generics;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Repos.Generics
{
    public class GenericRepo<TModel> : IGenericRepo<TModel> where TModel : BaseEntity
    {
        // tác vụ CRUD 
        protected DbSet<TModel> _dbSet;
        public GenericRepo(AppDbContext appDbContext)
        {
            _dbSet = appDbContext.Set<TModel>();
        }
        public async Task CreateAsync(TModel model)
        {
            await _dbSet.AddAsync(model);
        }

        public void Delete(TModel model)
        {
            _dbSet.Remove(model);
        }

        public async Task<IEnumerable<TModel>> ReadAllAsync()
        {
            return await _dbSet.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<TModel> ReadAsync(int id)
        {
            TModel? result = await _dbSet.FindAsync(id);
            if (result == null || result.IsDeleted)
                throw new Exceptions.InfrastructureException(HttpStatusCode.BadRequest, $"{result} not found");
            return result;
        }

        public void SoftDelete(TModel model)
        {
            model.IsDeleted = true;
        }

        public void Update(TModel model)
        {
            _dbSet.Update(model);
        }
    }
}
