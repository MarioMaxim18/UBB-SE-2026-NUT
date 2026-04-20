using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamNut.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetById(int id);
        Task<IEnumerable<T>> GetAll();
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(int id);
    }
}
