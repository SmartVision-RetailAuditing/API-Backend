using ApiBackend.DTOs;

namespace ApiBackend.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<NotificationDto>> GetNotificationsForRoleAsync(string role, int userId);

    }
}
