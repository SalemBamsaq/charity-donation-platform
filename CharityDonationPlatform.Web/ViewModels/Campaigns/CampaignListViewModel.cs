using System.Collections.Generic;

namespace CharityDonationPlatform.Web.ViewModels.Campaigns
{
    public class CampaignListViewModel
    {
        public IEnumerable<CampaignViewModel> Campaigns { get; set; } = new List<CampaignViewModel>();
        public string FilterStatus { get; set; }
        public int TotalCount { get; set; }
    }
}