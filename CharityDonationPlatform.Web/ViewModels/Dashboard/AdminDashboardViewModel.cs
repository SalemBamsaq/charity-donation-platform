using CharityDonationPlatform.Web.ViewModels.Campaigns;
using CharityDonationPlatform.Web.ViewModels.Donations;
using System.Collections.Generic;

namespace CharityDonationPlatform.Web.ViewModels.Dashboard
{
    public class AdminDashboardViewModel
    {
        public decimal TotalAmountRaised { get; set; }
        public int TotalDonorsCount { get; set; }
        public int ActiveCampaignsCount { get; set; }
        public int TotalCampaignsCount { get; set; }
        public IEnumerable<CampaignViewModel> RecentCampaigns { get; set; } = new List<CampaignViewModel>();
        public IEnumerable<DonationViewModel> RecentDonations { get; set; } = new List<DonationViewModel>();
        public IEnumerable<KeyValuePair<string, decimal>> TopDonors { get; set; } = new List<KeyValuePair<string, decimal>>();
        public IEnumerable<KeyValuePair<string, decimal>> TopCampaigns { get; set; } = new List<KeyValuePair<string, decimal>>();
    }
}