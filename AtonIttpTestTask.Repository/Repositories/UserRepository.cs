using AtonIttpTestTask.Domain.Interfaces;
using AtonIttpTestTask.Domain.Models;
using AtonIttpTestTask.Repository.DataBases;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtonIttpTestTask.Repository.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(User entity)
        {
            await _context.Users.AddAsync(entity);
        }

        public void Delete(User entity)
        {
            _context.Users.Remove(entity);
        }

        public async Task<IEnumerable<User>> GetAllActiveInDescendingOrderOfCreatedOnAsync()
        {
            return await _context.Users
                .Where(x => x.RevokedOn == null)
                .OrderByDescending(x => x.CreatedOn)
                .ToArrayAsync();
        }

        public async Task<User?> GetByLoginAndPasswordAsync(string login, string password)
        {
            return await _context.Users.Where(x => x.Login == login && x.Password == password).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByLoginAsync(string login)
        {
            return await _context.Users.Where(x => x.Login == login).FirstOrDefaultAsync();
        }

        public async Task<bool> IsLoginAreAvailableAsync(string login)
        {
            return !await _context.Users.AnyAsync(x => x.Login == login);
        }

        public void Update(User entity)
        {
            _context.Users.Update(entity);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetOlderThanAsync(int age)
        {
            var withBirthday = await _context.Users.Where(x => x.Birthday != null).ToListAsync();
            return withBirthday.Where(x => CalculateAge(x.Birthday.Value) > age);
        }

        private int CalculateAge(DateTime birthdate)
        {
            DateTime today = DateTime.Today;
            int age = today.Year - birthdate.Year;
            if (birthdate.AddYears(age) > today)
            {
                age--;
            }
            return age;
        }

        //public async Task<IEnumerable<User>> GetAllWhereAsync(Func<User, bool> predicate)
        //{
        //    return await _context.Users.Where(predicate).AsQueryable().ToArrayAsync();
        //}
    }
}
