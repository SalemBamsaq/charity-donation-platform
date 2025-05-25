using CharityDonationPlatform.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(Notification notification);
        Task CreateNotificationAsync(string userId, string title, string message, string type, string relatedEntityId);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId);
        Task<int> GetUnreadNotificationCountAsync(string userId);
        Task MarkNotificationAsReadAsync(int notificationId);
        Task MarkAllNotificationsAsReadAsync(string userId);
    }
}