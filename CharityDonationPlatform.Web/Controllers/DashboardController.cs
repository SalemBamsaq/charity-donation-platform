using CharityDonationPlatform.Application.Interfaces;
using CharityDonationPlatform.Domain.Enums;
using CharityDonationPlatform.Domain.Models;
using CharityDonationPlatform.Web.ViewModels.Campaigns;
using CharityDonationPlatform.Web.ViewModels.Dashboard;
using CharityDonationPlatform.Web.ViewModels.Donations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ICampaignService _campaignService;
        private readonly IDonationService _donationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ICampaignService campaignService,
            IDonationService donationService,
            UserManager<ApplicationUser> userManager)
        {
            _campaignService = campaignService;
            _donationService = donationService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // Redirect to the appropriate dashboard based on user role
            if (User.IsInRole(UserRoles.Admin) || User.IsInRole(UserRoles.Staff))
            {
                return RedirectToAction(nameof(Admin));
            }
            else // Default to donor dashboard
            {
                return RedirectToAction(nameof(Donor));
            }
        }

        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Staff)]
        public async Task<IActionResult> Admin()
        {
            var allCampaigns = await _campaignService.GetAllCampaignsAsync();
            var recentCampaigns = allCampaigns.OrderByDescending(c => c.Created).Take(5);

            // Get all donations for recent donations display
            var allDonations = new List<Donation>();
            foreach (var campaign in allCampaigns)
            {
                var campaignDonations = await _donationService.GetDonationsByCampaignIdAsync(campaign.Id);
                allDonations.AddRange(campaignDonations);
            }

            var recentDonations = allDonations
                .OrderByDescending(d => d.DonationDate)
                .Take(10);

            var topDonors = await _donationService.GetTopDonorsAsync(5);

            var topCampaigns = allCampaigns
                .OrderByDescending(c => c.AmountRaised)
                .Take(5)
                .Select(c => new KeyValuePair<string, decimal>(c.Title, c.AmountRaised));

            var viewModel = new AdminDashboardViewModel
            {
                TotalAmountRaised = await _campaignService.GetTotalAmountRaisedAsync(),
                TotalDonorsCount = await _campaignService.GetTotalDonorsCountAsync(),
                ActiveCampaignsCount = await _campaignService.GetActiveCampaignsCountAsync(),
                TotalCampaignsCount = allCampaigns.Count(),
                RecentCampaigns = recentCampaigns.Select(c => new CampaignViewModel
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
                RecentDonations = recentDonations.Select(d => new DonationViewModel
                {
                    Id = d.Id,
                    CampaignId = d.CampaignId,
                    CampaignTitle = d.Campaign?.Title,
                    DonorId = d.DonorId,
                    DonorName = $"{d.Donor?.FirstName} {d.Donor?.LastName}",
                    Amount = d.Amount,
                    DonationDate = d.DonationDate,
                    IsAnonymous = d.IsAnonymous,
                    Status = d.Status
                }),
                TopDonors = topDonors,
                TopCampaigns = topCampaigns
            };

            return View(viewModel);
        }

        [Authorize(Roles = UserRoles.Donor)]
        public async Task<IActionResult> Donor()
        {
            var userId = _userManager.GetUserId(User);
            var donations = await _donationService.GetDonationsByUserIdAsync(userId);

            // Get campaigns that this donor has supported
            var supportedCampaignIds = donations.Select(d => d.CampaignId).Distinct().ToList();
            var allCampaigns = await _campaignService.GetAllCampaignsAsync();
            var supportedCampaigns = allCampaigns.Where(c => supportedCampaignIds.Contains(c.Id));

            // Get recommended campaigns (campaigns that are active but not supported by this donor)
            var activeCampaigns = await _campaignService.GetActiveCampaignsAsync();
            var recommendedCampaigns = activeCampaigns
                .Where(c => !supportedCampaignIds.Contains(c.Id))
                .Take(3);

            var viewModel = new DonorDashboardViewModel
            {
                TotalDonationAmount = donations.Sum(d => d.Amount),
                TotalCampaignsSupported = supportedCampaignIds.Count,
                RecentDonations = donations
                    .OrderByDescending(d => d.DonationDate)
                    .Take(5)
                    .Select(d => new DonationViewModel
                    {
                        Id = d.Id,
                        CampaignId = d.CampaignId,
                        CampaignTitle = d.Campaign?.Title,
                        Amount = d.Amount,
                        DonationDate = d.DonationDate,
                        IsAnonymous = d.IsAnonymous,
                        Status = d.Status
                    }),
                SupportedCampaigns = supportedCampaigns.Select(c => new CampaignViewModel
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
                    Created = c.Created
                }),
                RecommendedCampaigns = recommendedCampaigns.Select(c => new CampaignViewModel
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
                    Created = c.Created
                })
            };

            return View(viewModel);
        }

        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> ExportDonations()
        {
            // Get all campaigns and their donations
            var allCampaigns = await _campaignService.GetAllCampaignsAsync();
            var allDonations = new List<Donation>();

            foreach (var campaign in allCampaigns)
            {
                var campaignDonations = await _donationService.GetDonationsByCampaignIdAsync(campaign.Id);
                allDonations.AddRange(campaignDonations);
            }

            // Create CSV content
            var csv = new StringBuilder();
            csv.AppendLine("Campaign,Donor,Amount,Date,Status,Message");

            foreach (var donation in allDonations.OrderByDescending(d => d.DonationDate))
            {
                var donorName = donation.IsAnonymous
                    ? "Anonymous"
                    : $"{donation.Donor?.FirstName} {donation.Donor?.LastName}";

                var message = string.IsNullOrEmpty(donation.Message) ? "" : donation.Message.Replace("\"", "\"\"");

                csv.AppendLine($"\"{donation.Campaign?.Title}\",\"{donorName}\",{donation.Amount},{donation.DonationDate:yyyy-MM-dd HH:mm:ss},{donation.Status},\"{message}\"");
            }

            // Return as downloadable file
            byte[] bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"donations_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> ExportCampaigns()
        {
            // Get all campaigns
            var allCampaigns = await _campaignService.GetAllCampaignsAsync();

            // Create CSV content
            var csv = new StringBuilder();
            csv.AppendLine("Title,Target Amount,Amount Raised,Progress %,Start Date,End Date,Status,Created By,Created Date");

            foreach (var campaign in allCampaigns.OrderByDescending(c => c.Created))
            {
                var status = campaign.IsActive
                    ? (campaign.EndDate < DateTime.UtcNow ? "Ended" : "Active")
                    : "Inactive";

                var progressPercentage = campaign.TargetAmount > 0
                    ? (campaign.AmountRaised / campaign.TargetAmount * 100).ToString("F1")
                    : "0";

                csv.AppendLine($"\"{campaign.Title}\",{campaign.TargetAmount},{campaign.AmountRaised},{progressPercentage}%,{campaign.StartDate:yyyy-MM-dd},{campaign.EndDate:yyyy-MM-dd},{status},\"{campaign.CreatedBy?.FirstName} {campaign.CreatedBy?.LastName}\",{campaign.Created:yyyy-MM-dd HH:mm:ss}");
            }

            // Return as downloadable file
            byte[] bytes = Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"campaigns_{DateTime.UtcNow:yyyyMMdd}.csv");
        }
    }
}