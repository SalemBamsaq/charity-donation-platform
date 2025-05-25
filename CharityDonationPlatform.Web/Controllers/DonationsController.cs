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

        public DonationsController(
            IDonationService donationService,
            ICampaignService campaignService,
            IPaymentService paymentService,
            UserManager<ApplicationUser> userManager)
        {
            _donationService = donationService;
            _campaignService = campaignService;
            _paymentService = paymentService;
            _userManager = userManager;
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
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var campaign = await _campaignService.GetCampaignByIdAsync(model.CampaignId);

                if (campaign == null)
                {
                    return NotFound();
                }

                try
                {
                    // For demo purposes, we'll simulate payment processing
                    // In a real application, you would integrate with Stripe payment processing

                    // Create donation record
                    var donation = new Donation
                    {
                        CampaignId = model.CampaignId,
                        DonorId = userId,
                        Amount = model.Amount,
                        TransactionId = Guid.NewGuid().ToString(), // Simulate transaction ID
                        PaymentMethod = "Credit Card",
                        DonationDate = DateTime.UtcNow,
                        IsAnonymous = model.IsAnonymous,
                        Message = model.Message,
                        Status = "Completed"
                    };

                    await _donationService.CreateDonationAsync(donation);

                    return RedirectToAction(nameof(ThankYou), new { id = donation.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while processing your donation. Please try again.");
                    // Log the exception in a real application
                }
            }

            var campaignForError = await _campaignService.GetCampaignByIdAsync(model.CampaignId);
            ViewBag.CampaignTitle = campaignForError?.Title;
            ViewBag.CampaignImage = campaignForError?.ImageUrl;
            return View(model);
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