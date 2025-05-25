using CharityDonationPlatform.Web.ViewModels.Donations;
using System.Collections.Generic;

namespace CharityDonationPlatform.Web.ViewModels.Campaigns
{
    public class CampaignDetailViewModel
    {
        public CampaignViewModel Campaign { get; set; }
        public IEnumerable<CampaignUpdateViewModel> Updates { get; set; } = new List<CampaignUpdateViewModel>();
        public IEnumerable<DonationViewModel> RecentDonations { get; set; } = new List<DonationViewModel>();
        public DonationCreateViewModel NewDonation { get; set; }
        public CampaignUpdateCreateViewModel NewUpdate { get; set; }
    }
}