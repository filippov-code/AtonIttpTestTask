using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtonIttpTestTask.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task CreateAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task SaveAsync();
    }
}
