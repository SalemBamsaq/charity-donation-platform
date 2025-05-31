using System.ComponentModel.DataAnnotations;

namespace CharityDonationPlatform.Web.ViewModels.Donations
{
    public class DonationCreateViewModel
    {
        [Required]
        public int CampaignId { get; set; }

        [Required(ErrorMessage = "Please enter a donation amount")]
        [Range(1, 1000000, ErrorMessage = "Donation amount must be between $1 and $1,000,000")]
        [Display(Name = "Donation Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Donate Anonymously")]
        public bool IsAnonymous { get; set; }

        [StringLength(500, ErrorMessage = "Message cannot be longer than 500 characters")]
        [Display(Name = "Message of Support")]
        public string? Message { get; set; }

        // Payment-related properties (for future Stripe integration)
        public string? PaymentMethodId { get; set; }
        public string? PaymentIntentId { get; set; }
        public string Currency { get; set; } = "USD";
    }
}