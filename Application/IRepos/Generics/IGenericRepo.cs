using Domain;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.IRepos.Generics
{
    public interface IGenericRepo<TModel> where TModel : BaseEntity
    {
        Task CreateAsync(TModel model);
        Task<IEnumerable<TModel>> ReadAllAsync();
        Task<TModel> ReadAsync(int id);
        void Update(TModel model);
        void Delete(TModel model);
        void SoftDelete(TModel model);
    }
}
