using System.Collections.Generic;

namespace CharityDonationPlatform.Web.ViewModels.Notifications
{
    public class NotificationListViewModel
    {
        public IEnumerable<NotificationViewModel> Notifications { get; set; } = new List<NotificationViewModel>();
        public int UnreadCount { get; set; }
    }
}