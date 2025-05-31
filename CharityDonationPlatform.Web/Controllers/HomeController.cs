using CharityDonationPlatform.Application.Interfaces;
using CharityDonationPlatform.Web.ViewModels;
using CharityDonationPlatform.Web.ViewModels.Campaigns;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICampaignService _campaignService;

        public HomeController(ILogger<HomeController> logger, ICampaignService campaignService)
        {
            _logger = logger;
            _campaignService = campaignService;
        }

        public async Task<IActionResult> Index()
        {
            var activeCampaigns = await _campaignService.GetActiveCampaignsAsync();

            var viewModel = new CampaignListViewModel
            {
                Campaigns = activeCampaigns.Select(c => new CampaignViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    TargetAmount = c.TargetAmount,
                    AmountRaised = c.AmountRaised,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    ImageUrl = c.ImageUrl,
                    IsActive = c.IsActive,
                    CreatedByName = $"{c.CreatedBy?.FirstName} {c.CreatedBy?.LastName}",
                    Created = c.Created
                }),
                TotalCount = activeCampaigns.Count()
            };

            return View(viewModel); // This should match @model CampaignListViewModel in Index.cshtml
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}