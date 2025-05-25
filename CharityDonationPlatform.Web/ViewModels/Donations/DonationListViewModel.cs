using System.Collections.Generic;

namespace CharityDonationPlatform.Web.ViewModels.Donations
{
    public class DonationListViewModel
    {
        public IEnumerable<DonationViewModel> Donations { get; set; } = new List<DonationViewModel>();
        public decimal TotalAmount { get; set; }
        public int TotalCount { get; set; }
    }
}