using System.ComponentModel.DataAnnotations;

namespace CharityDonationPlatform.Web.ViewModels.UserManagement
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please select a role")]
        [Display(Name = "User Role")]
        public string Role { get; set; }

        [Display(Name = "Account Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; } = true;
    }
}