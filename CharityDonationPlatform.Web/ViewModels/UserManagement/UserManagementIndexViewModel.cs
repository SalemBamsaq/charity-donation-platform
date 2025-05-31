using System.Collections.Generic;

namespace CharityDonationPlatform.Web.ViewModels.UserManagement
{
    public class UserManagementIndexViewModel
    {
        public IEnumerable<UserListViewModel> Users { get; set; } = new List<UserListViewModel>();
        public string SearchTerm { get; set; } = string.Empty;
        public string RoleFilter { get; set; } = string.Empty;
        public int TotalUsers { get; set; }
        public int AdminCount { get; set; }
        public int StaffCount { get; set; }
        public int DonorCount { get; set; }
    }
}