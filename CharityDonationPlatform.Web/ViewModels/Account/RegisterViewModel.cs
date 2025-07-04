﻿using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CharityDonationPlatform.Web.ViewModels.Account
{
    public class RegisterViewModel
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

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        // Explicitly mark as NOT required
        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }

        // Explicitly mark as NOT required and exclude from validation
        public string? ProfilePictureUrl { get; set; }
    }
}