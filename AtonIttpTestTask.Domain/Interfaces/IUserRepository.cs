using AtonIttpTestTask.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtonIttpTestTask.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByLoginAsync(string login);
        Task<User?> GetByLoginAndPasswordAsync(string login, string password);
        Task<IEnumerable<User>> GetAllActiveInDescendingOrderOfCreatedOnAsync();
        Task<bool> IsLoginAreAvailableAsync(string login);
        Task<IEnumerable<User>> GetOlderThanAsync(int age);


    }
}
