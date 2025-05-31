using CharityDonationPlatform.Application.Interfaces;
using CharityDonationPlatform.Domain.Enums;
using CharityDonationPlatform.Web.ViewModels.Dashboard;
using CharityDonationPlatform.Web.ViewModels.Campaigns;
using CharityDonationPlatform.Web.ViewModels.Donations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CharityDonationPlatform.Domain.Models;
using System.Linq;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ICampaignService _campaignService;
        private readonly IDonationService _donationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ICampaignService campaignService,
            IDonationService donationService,
            UserManager<ApplicationUser> userManager,
            ILogger<DashboardController> logger)
        {
            _campaignService = campaignService;
            _donationService = donationService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User);

                // Redirect based on user role
                if (User.IsInRole(UserRoles.Admin))
                {
                    return RedirectToAction(nameof(Admin));
                }
                else if (User.IsInRole(UserRoles.Staff))
                {
                    return RedirectToAction(nameof(Staff));
                }
                else
                {
                    return RedirectToAction(nameof(Donor));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining dashboard redirect for user {UserId}", _userManager.GetUserId(User));
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Admin()
        {
            try
            {
                var totalAmountRaised = await _campaignService.GetTotalAmountRaisedAsync();
                var totalDonorsCount = await _campaignService.GetTotalDonorsCountAsync();
                var activeCampaignsCount = await _campaignService.GetActiveCampaignsCountAsync();
                var allCampaigns = await _campaignService.GetAllCampaignsAsync();

                // Get recent campaigns (last 5)
                var recentCampaigns = allCampaigns
                    .OrderByDescending(c => c.Created)
                    .Take(5)
                    .Select(c => new CampaignViewModel
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
                    });

                // Get recent donations (last 10)
                var allDonations = new List<DonationViewModel>();
                foreach (var campaign in allCampaigns)
                {
                    var campaignDonations = await _donationService.GetDonationsByCampaignIdAsync(campaign.Id);
                    allDonations.AddRange(campaignDonations.Select(d => new DonationViewModel
                    {
                        Id = d.Id,
                        CampaignId = d.CampaignId,
                        CampaignTitle = campaign.Title,
                        DonorId = d.DonorId,
                        DonorName = d.IsAnonymous ? "Anonymous" : $"{d.Donor?.FirstName} {d.Donor?.LastName}",
                        Amount = d.Amount,
                        DonationDate = d.DonationDate,
                        IsAnonymous = d.IsAnonymous,
                        Message = d.Message,
                        Status = d.Status
                    }));
                }

                var recentDonations = allDonations
                    .OrderByDescending(d => d.DonationDate)
                    .Take(10);

                // Get top donors
                var topDonors = await _donationService.GetTopDonorsAsync(5);

                // Get top campaigns by amount raised
                var topCampaigns = allCampaigns
                    .OrderByDescending(c => c.AmountRaised)
                    .Take(5)
                    .Select(c => new KeyValuePair<string, decimal>(c.Title, c.AmountRaised));

                var viewModel = new AdminDashboardViewModel
                {
                    TotalAmountRaised = totalAmountRaised,
                    TotalDonorsCount = totalDonorsCount,
                    ActiveCampaignsCount = activeCampaignsCount,
                    TotalCampaignsCount = allCampaigns.Count(),
                    RecentCampaigns = recentCampaigns,
                    RecentDonations = recentDonations,
                    TopDonors = topDonors,
                    TopCampaigns = topCampaigns
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading admin dashboard");
                TempData["ErrorMessage"] = "Unable to load dashboard. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = UserRoles.Staff)]
        public async Task<IActionResult> Staff()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var allCampaigns = await _campaignService.GetAllCampaignsAsync();

                // Get campaigns created by this staff member
                var staffCampaigns = allCampaigns
                    .Where(c => c.CreatedById == userId)
                    .Select(c => new CampaignViewModel
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
                    });

                var viewModel = new
                {
                    StaffCampaigns = staffCampaigns,
                    TotalCampaigns = staffCampaigns.Count(),
                    TotalAmountRaised = staffCampaigns.Sum(c => c.AmountRaised),
                    ActiveCampaigns = staffCampaigns.Count(c => c.IsActive)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading staff dashboard for user {UserId}", _userManager.GetUserId(User));
                TempData["ErrorMessage"] = "Unable to load dashboard. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize(Roles = UserRoles.Donor)]
        public async Task<IActionResult> Donor()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var userDonations = await _donationService.GetDonationsByUserIdAsync(userId);

                var recentDonations = userDonations
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
                        Message = d.Message,
                        Status = d.Status
                    });

                // Get campaigns the user has supported
                var supportedCampaignIds = userDonations.Select(d => d.CampaignId).Distinct();
                var allCampaigns = await _campaignService.GetAllCampaignsAsync();
                var supportedCampaigns = allCampaigns
                    .Where(c => supportedCampaignIds.Contains(c.Id))
                    .Select(c => new CampaignViewModel
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
                    });

                // Get recommended campaigns (active campaigns not yet supported)
                var recommendedCampaigns = allCampaigns
                    .Where(c => c.IsActive && !supportedCampaignIds.Contains(c.Id))
                    .OrderByDescending(c => c.Created)
                    .Take(5)
                    .Select(c => new CampaignViewModel
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
                    });

                var viewModel = new DonorDashboardViewModel
                {
                    TotalDonationAmount = userDonations.Sum(d => d.Amount),
                    TotalCampaignsSupported = supportedCampaigns.Count(),
                    RecentDonations = recentDonations,
                    SupportedCampaigns = supportedCampaigns,
                    RecommendedCampaigns = recommendedCampaigns
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading donor dashboard for user {UserId}", _userManager.GetUserId(User));
                TempData["ErrorMessage"] = "Unable to load dashboard. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}