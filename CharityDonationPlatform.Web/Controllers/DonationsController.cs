using CharityDonationPlatform.Application.Interfaces;
using CharityDonationPlatform.Domain.Models;
using CharityDonationPlatform.Web.ViewModels.Donations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Web.Controllers
{
    [Authorize]
    public class DonationsController : Controller
    {
        private readonly IDonationService _donationService;
        private readonly ICampaignService _campaignService;
        private readonly IPaymentService _paymentService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DonationsController> _logger;

        public DonationsController(
            IDonationService donationService,
            ICampaignService campaignService,
            IPaymentService paymentService,
            UserManager<ApplicationUser> userManager,
            ILogger<DonationsController> logger)
        {
            _donationService = donationService;
            _campaignService = campaignService;
            _paymentService = paymentService;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var donations = await _donationService.GetDonationsByUserIdAsync(userId);

            var viewModel = new DonationListViewModel
            {
                Donations = donations.Select(d => new DonationViewModel
                {
                    Id = d.Id,
                    CampaignId = d.CampaignId,
                    CampaignTitle = d.Campaign?.Title,
                    DonorId = d.DonorId,
                    DonorName = $"{d.Donor?.FirstName} {d.Donor?.LastName}",
                    Amount = d.Amount,
                    TransactionId = d.TransactionId,
                    PaymentMethod = d.PaymentMethod,
                    DonationDate = d.DonationDate,
                    IsAnonymous = d.IsAnonymous,
                    Message = d.Message,
                    Status = d.Status
                }),
                TotalAmount = donations.Sum(d => d.Amount),
                TotalCount = donations.Count()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Donate(int campaignId)
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(campaignId);
            if (campaign == null)
            {
                return NotFound();
            }

            var model = new DonationCreateViewModel
            {
                CampaignId = campaignId
            };

            ViewBag.CampaignTitle = campaign.Title;
            ViewBag.CampaignImage = campaign.ImageUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Donate(DonationCreateViewModel model)
        {
            try
            {
                // FIXED: Add comprehensive logging for debugging
                _logger.LogInformation("Donation attempt - CampaignId: {CampaignId}, Amount: {Amount}, User: {UserId}",
                    model.CampaignId, model.Amount, _userManager.GetUserId(User));

                // FIXED: Check ModelState first and log validation errors
                if (!ModelState.IsValid)
                {
                    foreach (var modelState in ModelState)
                    {
                        foreach (var error in modelState.Value.Errors)
                        {
                            _logger.LogWarning("ModelState Error - Field: {Field}, Error: {Error}",
                                modelState.Key, error.ErrorMessage);
                        }
                    }

                    ModelState.AddModelError(string.Empty, "Please correct the errors below and try again.");

                    // Reload campaign data for the view
                    var campaignForValidation = await _campaignService.GetCampaignByIdAsync(model.CampaignId);
                    ViewBag.CampaignTitle = campaignForValidation?.Title ?? "Unknown Campaign";
                    ViewBag.CampaignImage = campaignForValidation?.ImageUrl;
                    return View(model);
                }

                var userId = _userManager.GetUserId(User);
                var campaign = await _campaignService.GetCampaignByIdAsync(model.CampaignId);

                // FIXED: Better campaign validation
                if (campaign == null)
                {
                    _logger.LogWarning("Donation attempt for non-existent campaign: {CampaignId}", model.CampaignId);
                    ModelState.AddModelError(string.Empty, "Campaign not found.");
                    return View(model);
                }

                if (!campaign.IsActive)
                {
                    _logger.LogWarning("Donation attempt for inactive campaign: {CampaignId}", model.CampaignId);
                    ModelState.AddModelError(string.Empty, "This campaign is no longer active.");
                    ViewBag.CampaignTitle = campaign.Title;
                    ViewBag.CampaignImage = campaign.ImageUrl;
                    return View(model);
                }

                if (DateTime.UtcNow > campaign.EndDate)
                {
                    _logger.LogWarning("Donation attempt for ended campaign: {CampaignId}, EndDate: {EndDate}",
                        model.CampaignId, campaign.EndDate);
                    ModelState.AddModelError(string.Empty, "This campaign has ended.");
                    ViewBag.CampaignTitle = campaign.Title;
                    ViewBag.CampaignImage = campaign.ImageUrl;
                    return View(model);
                }

                // FIXED: Additional amount validation
                if (model.Amount < 1)
                {
                    ModelState.AddModelError(nameof(model.Amount), "Donation amount must be at least $1.");
                    ViewBag.CampaignTitle = campaign.Title;
                    ViewBag.CampaignImage = campaign.ImageUrl;
                    return View(model);
                }

                if (model.Amount > 1000000)
                {
                    ModelState.AddModelError(nameof(model.Amount), "Donation amount cannot exceed $1,000,000.");
                    ViewBag.CampaignTitle = campaign.Title;
                    ViewBag.CampaignImage = campaign.ImageUrl;
                    return View(model);
                }

                // FIXED: Create donation record with better error handling
                var donation = new Donation
                {
                    CampaignId = model.CampaignId,
                    DonorId = userId,
                    Amount = model.Amount,
                    TransactionId = Guid.NewGuid().ToString(), // Simulate transaction ID
                    PaymentMethod = "Credit Card",
                    DonationDate = DateTime.UtcNow,
                    IsAnonymous = model.IsAnonymous,
                    Message = model.Message ?? string.Empty,
                    Status = "Completed"
                };

                _logger.LogInformation("Creating donation: {DonationData}", new
                {
                    donation.CampaignId,
                    donation.Amount,
                    donation.DonorId,
                    donation.IsAnonymous
                });

                await _donationService.CreateDonationAsync(donation);

                _logger.LogInformation("Donation created successfully: {DonationId}", donation.Id);
                TempData["SuccessMessage"] = "Your donation was successful! Thank you for your generosity.";
                return RedirectToAction(nameof(ThankYou), new { id = donation.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing donation for campaign {CampaignId} by user {UserId}",
                    model.CampaignId, _userManager.GetUserId(User));
                ModelState.AddModelError(string.Empty, "An error occurred while processing your donation. Please try again.");

                // Reload campaign data for the view
                try
                {
                    var campaignForError = await _campaignService.GetCampaignByIdAsync(model.CampaignId);
                    ViewBag.CampaignTitle = campaignForError?.Title ?? "Unknown Campaign";
                    ViewBag.CampaignImage = campaignForError?.ImageUrl;
                }
                catch (Exception reloadEx)
                {
                    _logger.LogError(reloadEx, "Failed to reload campaign data for error view");
                    ViewBag.CampaignTitle = "Unknown Campaign";
                    ViewBag.CampaignImage = null;
                }

                return View(model);
            }
        }

        public async Task<IActionResult> ThankYou(int id)
        {
            var donation = await _donationService.GetDonationByIdAsync(id);
            if (donation == null)
            {
                return NotFound();
            }

            // Verify that the current user is the owner of this donation
            var userId = _userManager.GetUserId(User);
            if (donation.DonorId != userId)
            {
                return Forbid();
            }

            var viewModel = new DonationViewModel
            {
                Id = donation.Id,
                CampaignId = donation.CampaignId,
                CampaignTitle = donation.Campaign?.Title,
                DonorId = donation.DonorId,
                DonorName = $"{donation.Donor?.FirstName} {donation.Donor?.LastName}",
                Amount = donation.Amount,
                TransactionId = donation.TransactionId,
                PaymentMethod = donation.PaymentMethod,
                DonationDate = donation.DonationDate,
                IsAnonymous = donation.IsAnonymous,
                Message = donation.Message,
                Status = donation.Status
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Receipt(int id)
        {
            var donation = await _donationService.GetDonationByIdAsync(id);
            if (donation == null)
            {
                return NotFound();
            }

            // Verify that the current user is the owner of this donation
            var userId = _userManager.GetUserId(User);
            if (donation.DonorId != userId)
            {
                return Forbid();
            }

            var viewModel = new DonationViewModel
            {
                Id = donation.Id,
                CampaignId = donation.CampaignId,
                CampaignTitle = donation.Campaign?.Title,
                DonorId = donation.DonorId,
                DonorName = $"{donation.Donor?.FirstName} {donation.Donor?.LastName}",
                Amount = donation.Amount,
                TransactionId = donation.TransactionId,
                PaymentMethod = donation.PaymentMethod,
                DonationDate = donation.DonationDate,
                IsAnonymous = donation.IsAnonymous,
                Message = donation.Message,
                Status = donation.Status
            };

            return View(viewModel);
        }
    }
}