using System;
using System.ComponentModel.DataAnnotations;

namespace CharityDonationPlatform.Domain.Models
{
    public class CampaignUpdate
    {
        public int Id { get; set; }

        public int CampaignId { get; set; }

        public virtual Campaign Campaign { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public string ImageUrl { get; set; }

        public string CreatedById { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Updated { get; set; }
    }
}