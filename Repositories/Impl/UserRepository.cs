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

        public async Task<IEnumerable<User>> GetAllUsersAsync(int pageNumber, int pageSize)
        {
            return await _context.Users
                .OrderBy(u => u.FullName) // İsim sırasına göre
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
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

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _context.Users
                .Where(u => u.Role == role && u.IsActive == true) // Sadece aktif çalışanlar gelsin
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }
    }
}
