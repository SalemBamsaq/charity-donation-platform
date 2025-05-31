using System;

namespace CharityDonationPlatform.Web.ViewModels.UserManagement
{
    public class UserDetailsViewModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime Created { get; set; }
        public string ProfilePictureUrl { get; set; }

        public string FullName => $"{FirstName} {LastName}";
        public string StatusBadge => IsActive ? "Active" : "Inactive";
        public string StatusClass => IsActive ? "success" : "secondary";
        public string RoleBadgeClass => Role switch
        {
            "Admin" => "danger",
            "Staff" => "warning",
            _ => "primary"
        };
    }
}