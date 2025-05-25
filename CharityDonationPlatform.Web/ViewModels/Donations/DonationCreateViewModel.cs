using System.ComponentModel.DataAnnotations;

namespace CharityDonationPlatform.Web.ViewModels.Donations
{
    public class DonationCreateViewModel
    {
        public int CampaignId { get; set; }

        [Required]
        [Range(1, 1000000, ErrorMessage = "Donation amount must be at least $1.")]
        public decimal Amount { get; set; }

        [Display(Name = "Donate Anonymously")]
        public bool IsAnonymous { get; set; }

        [StringLength(500, ErrorMessage = "Message cannot be longer than 500 characters.")]
        public string Message { get; set; }

        // Payment-related properties
        public string PaymentMethodId { get; set; }
        public string PaymentIntentId { get; set; }
        public string Currency { get; set; } = "USD";
    }
}