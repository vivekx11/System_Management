using CollegeEventPortal.Models;

namespace CollegeEventPortal.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message, NotificationType type = NotificationType.Info);
        Task SendBulkNotificationAsync(List<string> userIds, string title, string message, NotificationType type = NotificationType.Info);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 10);
        Task MarkAsReadAsync(int notificationId);
    }
}
