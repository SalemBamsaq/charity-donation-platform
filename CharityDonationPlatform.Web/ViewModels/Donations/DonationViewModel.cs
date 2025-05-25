using System;

namespace CharityDonationPlatform.Web.ViewModels.Donations
{
    public class DonationViewModel
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string CampaignTitle { get; set; }
        public string DonorId { get; set; }
        public string DonorName { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime DonationDate { get; set; }
        public bool IsAnonymous { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }

        public string DisplayDonorName => IsAnonymous ? "Anonymous" : DonorName;
    }
}