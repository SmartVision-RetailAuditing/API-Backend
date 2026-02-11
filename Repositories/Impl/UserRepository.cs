using ApiBackend.Data;
using ApiBackend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using ApiBackend.Entities;

namespace ApiBackend.Repositories.Impl
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task AddUserAsync(User user) { 
            await _context.Users.AddAsync(user); 
            await _context.SaveChangesAsync(); 
        }
        public async Task UpdateUserAsync(User user) { 
            _context.Users.Update(user); 
            await _context.SaveChangesAsync(); 
        }
        public async Task DeleteUserAsync(User user) { 
            _context.Users.Remove(user); 
            await _context.SaveChangesAsync(); 
        }
    }
}
