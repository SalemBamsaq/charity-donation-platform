using CharityDonationPlatform.Application.Interfaces;
using CharityDonationPlatform.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = UserRoles.Admin)]
    public class DonationsApiController : ControllerBase
    {
        private readonly IDonationService _donationService;
        private readonly ICampaignService _campaignService;

        public DonationsApiController(
            IDonationService donationService,
            ICampaignService campaignService)
        {
            _donationService = donationService;
            _campaignService = campaignService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDonationStats()
        {
            var totalAmount = await _donationService.GetTotalDonationAmountAsync();
            var donorCount = await _campaignService.GetTotalDonorsCountAsync();
            var activeCount = await _campaignService.GetActiveCampaignsCountAsync();

            return Ok(new
            {
                totalAmount,
                donorCount,
                activeCount
            });
        }

        [HttpGet("topDonors")]
        public async Task<IActionResult> GetTopDonors(int count = 5)
        {
            var topDonors = await _donationService.GetTopDonorsAsync(count);
            return Ok(topDonors);
        }

        [HttpGet("byCampaign/{campaignId}")]
        public async Task<IActionResult> GetDonationsByCampaign(int campaignId)
        {
            var donations = await _donationService.GetDonationsByCampaignIdAsync(campaignId);
            return Ok(donations.Select(d => new
            {
                d.Id,
                DonorName = d.IsAnonymous ? "Anonymous" : $"{d.Donor?.FirstName} {d.Donor?.LastName}",
                d.Amount,
                Date = d.DonationDate,
                d.Message,
                d.Status
            }));
        }
    }
}