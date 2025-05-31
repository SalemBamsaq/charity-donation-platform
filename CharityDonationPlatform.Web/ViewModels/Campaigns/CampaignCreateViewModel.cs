using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace CharityDonationPlatform.Web.ViewModels.Campaigns
{
    public class CampaignCreateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Campaign title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Campaign description is required")]
        [StringLength(5000, ErrorMessage = "Description cannot be longer than 5000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Target amount is required")]
        [Range(1, 1000000000, ErrorMessage = "Target amount must be between $1 and $1,000,000,000")]
        [Display(Name = "Target Amount")]
        public decimal TargetAmount { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

        [Required(ErrorMessage = "End date is required")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.Date.AddMonths(1);

        [Display(Name = "Campaign Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Custom validation to ensure end date is after start date
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "End date must be after start date",
                    new[] { nameof(EndDate) });
            }

            if (StartDate < DateTime.UtcNow.Date)
            {
                yield return new ValidationResult(
                    "Start date cannot be in the past",
                    new[] { nameof(StartDate) });
            }
        }
    }
}