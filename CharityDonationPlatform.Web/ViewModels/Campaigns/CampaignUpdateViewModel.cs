using System;

namespace CharityDonationPlatform.Web.ViewModels.Campaigns
{
    public class CampaignUpdateViewModel
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public string CreatedByName { get; set; }
        public DateTime Created { get; set; }
    }
}