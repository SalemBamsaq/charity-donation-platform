using System;

namespace CharityDonationPlatform.Domain.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public string Type { get; set; } // e.g., "DonationReceived", "CampaignGoalReached", etc.

        public string RelatedEntityId { get; set; } // e.g., "CampaignId:123" or "DonationId:456"
    }
}