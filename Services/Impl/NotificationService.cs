using ApiBackend.DTOs;
using ApiBackend.Repositories.Interfaces;
using ApiBackend.Services.Interfaces;

namespace ApiBackend.Services.Impl
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repo;

        public NotificationService(INotificationRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<NotificationDto>> GetNotificationsAsync(string role, int userId)
        {
            return await _repo.GetNotificationsForRoleAsync(role, userId);
        }
    }
}