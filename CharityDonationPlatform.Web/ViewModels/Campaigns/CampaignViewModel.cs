using System;
using System.ComponentModel.DataAnnotations;

namespace CharityDonationPlatform.Web.ViewModels.Campaigns
{
    public class CampaignViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(1, 1000000000, ErrorMessage = "Target amount must be greater than 0.")]
        [Display(Name = "Target Amount")]
        public decimal TargetAmount { get; set; }

        [Display(Name = "Amount Raised")]
        public decimal AmountRaised { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Created By")]
        public string CreatedByName { get; set; }

        public string CreatedById { get; set; }

        [Display(Name = "Created On")]
        public DateTime Created { get; set; }

        // Calculated Properties
        public int DaysLeft => (EndDate - DateTime.UtcNow).Days > 0 ? (EndDate - DateTime.UtcNow).Days : 0;

        public decimal ProgressPercentage => TargetAmount > 0 ? Math.Min((AmountRaised / TargetAmount) * 100, 100) : 0;

        public string Status
        {
            get
            {
                if (!IsActive)
                    return "Inactive";
                else if (DateTime.UtcNow < StartDate)
                    return "Upcoming";
                else if (DateTime.UtcNow > EndDate)
                    return "Ended";
                else if (AmountRaised >= TargetAmount)
                    return "Funded";
                else
                    return "Active";
            }
        }
    }
}