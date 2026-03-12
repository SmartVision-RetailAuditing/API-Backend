using ApiBackend.DTOs;

namespace ApiBackend.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetNotificationsAsync(string role, int userId);
    }
}