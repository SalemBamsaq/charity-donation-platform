using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CharityDonationPlatform.Web.ViewModels.Account
{
    public class ProfileViewModel
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Profile Picture")]
        public IFormFile ProfilePicture { get; set; }

        public string ProfilePictureUrl { get; set; }
    }
}