using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CharityDonationPlatform.Web.ViewModels.Campaigns
{
    public class CampaignUpdateCreateViewModel
    {
        public int CampaignId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Display(Name = "Update Image")]
        public IFormFile ImageFile { get; set; }
    }
}