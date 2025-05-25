using CharityDonationPlatform.Web.ViewModels.Campaigns;
using CharityDonationPlatform.Web.ViewModels.Donations;
using System.Collections.Generic;

namespace CharityDonationPlatform.Web.ViewModels.Dashboard
{
    public class DonorDashboardViewModel
    {
        public decimal TotalDonationAmount { get; set; }
        public int TotalCampaignsSupported { get; set; }
        public IEnumerable<DonationViewModel> RecentDonations { get; set; } = new List<DonationViewModel>();
        public IEnumerable<CampaignViewModel> SupportedCampaigns { get; set; } = new List<CampaignViewModel>();
        public IEnumerable<CampaignViewModel> RecommendedCampaigns { get; set; } = new List<CampaignViewModel>();
    }
}