using System;

namespace CharityDonationPlatform.Web.ViewModels.Notifications
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime Created { get; set; }
        public string Type { get; set; }
        public string RelatedEntityId { get; set; }

        // Display properties
        public string TimeAgo
        {
            get
            {
                var span = DateTime.UtcNow - Created;

                if (span.TotalDays > 30)
                    return $"{(int)(span.TotalDays / 30)} month(s) ago";
                if (span.TotalDays > 1)
                    return $"{(int)span.TotalDays} day(s) ago";
                if (span.TotalHours > 1)
                    return $"{(int)span.TotalHours} hour(s) ago";
                if (span.TotalMinutes > 1)
                    return $"{(int)span.TotalMinutes} minute(s) ago";

                return "Just now";
            }
        }

        public string IconClass
        {
            get
            {
                return Type switch
                {
                    "DonationReceived" => "fa-donate text-success",
                    "DonationMade" => "fa-heart text-danger",
                    "CampaignCreated" => "fa-bullhorn text-primary",
                    "CampaignUpdate" => "fa-newspaper text-info",
                    "CampaignGoalReached" => "fa-trophy text-warning",
                    _ => "fa-bell text-secondary"
                };
            }
        }
    }
}