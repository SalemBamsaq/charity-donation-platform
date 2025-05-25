using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CharityDonationPlatform.Domain.Models
{
    public class Campaign
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(1, double.MaxValue)]
        public decimal TargetAmount { get; set; }

        public decimal AmountRaised { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public string ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public string CreatedById { get; set; }

        public virtual ApplicationUser CreatedBy { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Updated { get; set; }

        public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

        public virtual ICollection<CampaignUpdate> Updates { get; set; } = new List<CampaignUpdate>();
    }
}