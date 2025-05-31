using CharityDonationPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CharityDonationPlatform.Domain.Models;

namespace CharityDonationPlatform.Web.Controllers.Api
{
    [Route("api/Notifications")] // Changed from "api/[controller]" to match what site.js expects
    [ApiController]
    [Authorize]
    public class NotificationsApiController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsApiController(
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _userManager.GetUserId(User);

            // Handle case where user is not found
            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new { count = 0 });
            }

            var count = await _notificationService.GetUnreadNotificationCountAsync(userId);
            return Ok(new { count = count });
        }

        [HttpPost("{id}/markAsRead")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkNotificationAsReadAsync(id);
            return Ok();
        }
    }
}