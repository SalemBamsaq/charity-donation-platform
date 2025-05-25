using CharityDonationPlatform.Application.Interfaces;
using CharityDonationPlatform.Domain.Enums;
using CharityDonationPlatform.Domain.Models;
using CharityDonationPlatform.Web.ViewModels.Campaigns;
using CharityDonationPlatform.Web.ViewModels.Donations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Web.Controllers
{
    public class CampaignsController : Controller
    {
        private readonly ICampaignService _campaignService;
        private readonly IDonationService _donationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CampaignsController(
            ICampaignService campaignService,
            IDonationService donationService,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _campaignService = campaignService;
            _donationService = donationService;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string status = "active")
        {
            var campaigns = status.ToLower() switch
            {
                "all" => await _campaignService.GetAllCampaignsAsync(),
                _ => await _campaignService.GetActiveCampaignsAsync()
            };

            var viewModel = new CampaignListViewModel
            {
                Campaigns = campaigns.Select(c => new CampaignViewModel
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
                FilterStatus = status,
                TotalCount = campaigns.Count()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound();
            }

            var recentDonations = await _donationService.GetDonationsByCampaignIdAsync(id);
            var updates = await _campaignService.GetCampaignUpdatesAsync(id);

            var viewModel = new CampaignDetailViewModel
            {
                Campaign = new CampaignViewModel
                {
                    Id = campaign.Id,
                    Title = campaign.Title,
                    Description = campaign.Description,
                    TargetAmount = campaign.TargetAmount,
                    AmountRaised = campaign.AmountRaised,
                    StartDate = campaign.StartDate,
                    EndDate = campaign.EndDate,
                    ImageUrl = campaign.ImageUrl,
                    IsActive = campaign.IsActive,
                    CreatedByName = $"{campaign.CreatedBy?.FirstName} {campaign.CreatedBy?.LastName}",
                    CreatedById = campaign.CreatedById,
                    Created = campaign.Created
                },
                RecentDonations = recentDonations.Take(10).Select(d => new DonationViewModel
                {
                    Id = d.Id,
                    CampaignId = d.CampaignId,
                    CampaignTitle = campaign.Title,
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
                Updates = updates.Select(u => new CampaignUpdateViewModel
                {
                    Id = u.Id,
                    CampaignId = u.CampaignId,
                    Title = u.Title,
                    Content = u.Content,
                    ImageUrl = u.ImageUrl,
                    CreatedByName = $"{u.CreatedBy?.FirstName} {u.CreatedBy?.LastName}",
                    Created = u.Created
                }),
                NewDonation = new DonationCreateViewModel
                {
                    CampaignId = campaign.Id
                },
                NewUpdate = new CampaignUpdateCreateViewModel
                {
                    CampaignId = campaign.Id
                }
            };

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Staff)]
        public IActionResult Create()
        {
            return View(new CampaignCreateViewModel());
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Staff)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CampaignCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var imageUrl = "/images/default-campaign.jpg";

                // Handle image upload if provided
                if (model.ImageFile != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/campaigns");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    imageUrl = $"/uploads/campaigns/{fileName}";
                }

                var campaign = new Campaign
                {
                    Title = model.Title,
                    Description = model.Description,
                    TargetAmount = model.TargetAmount,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    ImageUrl = imageUrl,
                    IsActive = model.IsActive
                };

                var userId = _userManager.GetUserId(User);
                await _campaignService.CreateCampaignAsync(campaign, userId);

                TempData["SuccessMessage"] = "Campaign created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Staff)]
        public async Task<IActionResult> Edit(int id)
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound();
            }

            // Check if user is admin or campaign creator
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole(UserRoles.Admin);
            if (!isAdmin && campaign.CreatedById != userId)
            {
                return Forbid();
            }

            var model = new CampaignCreateViewModel
            {
                Id = campaign.Id,
                Title = campaign.Title,
                Description = campaign.Description,
                TargetAmount = campaign.TargetAmount,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                ImageUrl = campaign.ImageUrl,
                IsActive = campaign.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Staff)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CampaignCreateViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var campaign = await _campaignService.GetCampaignByIdAsync(id);
                    if (campaign == null)
                    {
                        return NotFound();
                    }

                    // Check if user is admin or campaign creator
                    var userId = _userManager.GetUserId(User);
                    var isAdmin = User.IsInRole(UserRoles.Admin);
                    if (!isAdmin && campaign.CreatedById != userId)
                    {
                        return Forbid();
                    }

                    // Update properties
                    campaign.Title = model.Title;
                    campaign.Description = model.Description;
                    campaign.TargetAmount = model.TargetAmount;
                    campaign.StartDate = model.StartDate;
                    campaign.EndDate = model.EndDate;
                    campaign.IsActive = model.IsActive;

                    // Handle image upload if provided
                    if (model.ImageFile != null)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/campaigns");

                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(fileStream);
                        }

                        campaign.ImageUrl = $"/uploads/campaigns/{fileName}";
                    }

                    await _campaignService.UpdateCampaignAsync(campaign);
                    TempData["SuccessMessage"] = "Campaign updated successfully!";
                }
                catch (Exception)
                {
                    if (!await CampaignExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id });
            }
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound();
            }

            var viewModel = new CampaignViewModel
            {
                Id = campaign.Id,
                Title = campaign.Title,
                Description = campaign.Description,
                TargetAmount = campaign.TargetAmount,
                AmountRaised = campaign.AmountRaised,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                ImageUrl = campaign.ImageUrl,
                IsActive = campaign.IsActive,
                CreatedByName = $"{campaign.CreatedBy?.FirstName} {campaign.CreatedBy?.LastName}",
                Created = campaign.Created
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _campaignService.DeleteCampaignAsync(id);
            TempData["SuccessMessage"] = "Campaign deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = UserRoles.Admin + "," + UserRoles.Staff)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUpdate(CampaignUpdateCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var imageUrl = string.Empty;

                // Handle image upload if provided
                if (model.ImageFile != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/updates");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    imageUrl = $"/uploads/updates/{fileName}";
                }

                var update = new CampaignUpdate
                {
                    CampaignId = model.CampaignId,
                    Title = model.Title,
                    Content = model.Content,
                    ImageUrl = imageUrl
                };

                var userId = _userManager.GetUserId(User);
                await _campaignService.AddCampaignUpdateAsync(update, userId);

                TempData["SuccessMessage"] = "Campaign update added successfully!";
                return RedirectToAction(nameof(Details), new { id = model.CampaignId });
            }

            // If we got this far, something failed, redisplay form
            TempData["ErrorMessage"] = "Failed to add campaign update. Please try again.";
            return RedirectToAction(nameof(Details), new { id = model.CampaignId });
        }

        private async Task<bool> CampaignExists(int id)
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            return campaign != null;
        }
    }
}