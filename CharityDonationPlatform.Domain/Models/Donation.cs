using System;
using System.ComponentModel.DataAnnotations;

namespace CharityDonationPlatform.Domain.Models
{
    public class Donation
    {
        public int Id { get; set; }

        public int CampaignId { get; set; }

        public virtual Campaign Campaign { get; set; }

        public string DonorId { get; set; }

        public virtual ApplicationUser Donor { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public string TransactionId { get; set; }

        public string PaymentMethod { get; set; }

        public DateTime DonationDate { get; set; } = DateTime.UtcNow;

        public bool IsAnonymous { get; set; } = false;

        public string Message { get; set; }

        public string Status { get; set; } = "Completed";
    }
}