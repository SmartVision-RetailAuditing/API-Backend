using ApiBackend.Entities;

namespace ApiBackend.Repositories.Interfaces
{
    public interface IUserRepository
    {
        //Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<User>> GetAllUsersAsync(int pageNumber, int pageSize);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(int id);
        Task AddUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(User user);

        // Role göre kullanıcıları getiren metot
        // Supervisor'un task'e field_worker atayabilmesi için tüm field_workerleri görmesi gerekir.
        // GetAllUsers() metodundan farkı bu metodu sadece SUPERVISOR kullanacak ve tüm FIELD_WORKER'ları görecek
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role);
    }
}
