using CharityDonationPlatform.Domain.Enums;
using CharityDonationPlatform.Domain.Models;
using CharityDonationPlatform.Web.ViewModels.UserManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Web.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: UserManagement/Index
        public async Task<IActionResult> Index(string searchTerm = "", string roleFilter = "")
        {
            try
            {
                var users = await _userManager.Users
                    .Where(u => string.IsNullOrEmpty(searchTerm) ||
                               u.FirstName.Contains(searchTerm) ||
                               u.LastName.Contains(searchTerm) ||
                               u.Email.Contains(searchTerm))
                    .OrderBy(u => u.FirstName)
                    .ToListAsync();

                var userViewModels = new List<UserListViewModel>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var primaryRole = roles.FirstOrDefault() ?? "Donor";

                    // Filter by role if specified
                    if (!string.IsNullOrEmpty(roleFilter) && primaryRole != roleFilter)
                        continue;

                    userViewModels.Add(new UserListViewModel
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Role = primaryRole,
                        IsActive = user.IsActive,
                        Created = user.Created,
                        EmailConfirmed = user.EmailConfirmed
                    });
                }

                var model = new UserManagementIndexViewModel
                {
                    Users = userViewModels,
                    SearchTerm = searchTerm,
                    RoleFilter = roleFilter,
                    TotalUsers = userViewModels.Count,
                    AdminCount = userViewModels.Count(u => u.Role == UserRoles.Admin),
                    StaffCount = userViewModels.Count(u => u.Role == UserRoles.Staff),
                    DonorCount = userViewModels.Count(u => u.Role == UserRoles.Donor)
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["ErrorMessage"] = "Unable to load users. Please try again.";
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // GET: UserManagement/Create
        public IActionResult Create()
        {
            return View(new CreateUserViewModel());
        }

        // POST: UserManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Check if user with email already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
                    return View(model);
                }

                // Create new user
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true, // Admin-created users are automatically confirmed
                    ProfilePictureUrl = "/images/default-profile.png",
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign role to user
                    await _userManager.AddToRoleAsync(user, model.Role);

                    _logger.LogInformation("Admin {AdminId} created new user {UserId} with role {Role}",
                        _userManager.GetUserId(User), user.Id, model.Role);

                    TempData["SuccessMessage"] = $"User {user.FirstName} {user.LastName} created successfully as {model.Role}.";
                    return RedirectToAction(nameof(Index));
                }

                // Add Identity errors to ModelState
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the user. Please try again.");
            }

            return View(model);
        }

        // GET: UserManagement/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault() ?? UserRoles.Donor;

            var model = new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = currentRole,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed
            };

            return View(model);
        }

        // POST: UserManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Prevent admin from deactivating themselves
                var currentUserId = _userManager.GetUserId(User);
                if (user.Id == currentUserId && !model.IsActive)
                {
                    ModelState.AddModelError(nameof(model.IsActive), "You cannot deactivate your own account.");
                    return View(model);
                }

                // Update user properties
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.UserName = model.Email;
                user.IsActive = model.IsActive;
                user.EmailConfirmed = model.EmailConfirmed;

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    foreach (var error in updateResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                // Update user role if changed
                var currentRoles = await _userManager.GetRolesAsync(user);
                var currentRole = currentRoles.FirstOrDefault();

                if (currentRole != model.Role)
                {
                    // Remove from current role
                    if (!string.IsNullOrEmpty(currentRole))
                    {
                        await _userManager.RemoveFromRoleAsync(user, currentRole);
                    }

                    // Add to new role
                    await _userManager.AddToRoleAsync(user, model.Role);
                }

                _logger.LogInformation("Admin {AdminId} updated user {UserId}",
                    _userManager.GetUserId(User), user.Id);

                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the user. Please try again.");
            }

            return View(model);
        }

        // GET: UserManagement/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault() ?? UserRoles.Donor;

            var model = new UserDetailsViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = currentRole,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                Created = user.Created,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return View(model);
        }

        // POST: UserManagement/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                // Prevent admin from deactivating themselves
                var currentUserId = _userManager.GetUserId(User);
                if (user.Id == currentUserId)
                {
                    TempData["ErrorMessage"] = "You cannot change your own account status.";
                    return RedirectToAction(nameof(Index));
                }

                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);

                var status = user.IsActive ? "activated" : "deactivated";
                TempData["SuccessMessage"] = $"User {user.FirstName} {user.LastName} has been {status}.";

                _logger.LogInformation("Admin {AdminId} {Action} user {UserId}",
                    currentUserId, status, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status for {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the user status.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: UserManagement/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Generate password reset token and reset password
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin {AdminId} reset password for user {UserId}",
                        _userManager.GetUserId(User), user.Id);

                    return Json(new { success = true, message = "Password reset successfully." });
                }

                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Json(new { success = false, message = errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", id);
                return Json(new { success = false, message = "An error occurred while resetting the password." });
            }
        }
    }
}