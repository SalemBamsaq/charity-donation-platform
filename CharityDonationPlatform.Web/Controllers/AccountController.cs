using CharityDonationPlatform.Domain.Enums;
using CharityDonationPlatform.Domain.Models;
using CharityDonationPlatform.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CharityDonationPlatform.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if user already exists
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError(string.Empty, "A user with this email already exists.");
                        return View(model);
                    }

                    var profilePictureUrl = "/images/default-profile.png";

                    // Handle profile picture upload
                    if (model.ProfilePicture != null)
                    {
                        try
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/profiles");

                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            var filePath = Path.Combine(uploadsFolder, fileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await model.ProfilePicture.CopyToAsync(fileStream);
                            }

                            profilePictureUrl = $"/uploads/profiles/{fileName}";
                        }
                        catch (Exception ex)
                        {
                            // Log the error but don't fail registration for profile picture issues
                            ModelState.AddModelError(string.Empty, "Profile picture upload failed, but registration will continue with default image.");
                            profilePictureUrl = "/images/default-profile.png";
                        }
                    }

                    var user = new ApplicationUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        ProfilePictureUrl = profilePictureUrl ?? "/images/default-profile.png",
                        EmailConfirmed = true // Since you're auto-confirming emails
                    };

                    // Create the user first
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        try
                        {
                            // Ensure the Donor role exists before adding user to it
                            if (!await _roleManager.RoleExistsAsync(UserRoles.Donor))
                            {
                                await _roleManager.CreateAsync(new IdentityRole(UserRoles.Donor));
                            }

                            // Add user to Donor role
                            var roleResult = await _userManager.AddToRoleAsync(user, UserRoles.Donor);
                            if (!roleResult.Succeeded)
                            {
                                // Log role assignment errors but don't fail registration
                                foreach (var error in roleResult.Errors)
                                {
                                    ModelState.AddModelError(string.Empty, $"Role assignment failed: {error.Description}");
                                }
                            }

                            // Generate and confirm email token (since you're auto-confirming)
                            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            await _userManager.ConfirmEmailAsync(user, token);

                            // Sign in the user
                            await _signInManager.SignInAsync(user, isPersistent: false);

                            return RedirectToAction("Index", "Home");
                        }
                        catch (Exception ex)
                        {
                            // If role assignment fails, still allow the user to be created
                            // but add an error message
                            ModelState.AddModelError(string.Empty, "Account created successfully, but there was an issue with role assignment. Please contact support if you experience any issues.");

                            // Still sign them in
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        // Add all Identity errors to ModelState
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
                else
                {
                    // Add a general error message if ModelState is invalid
                    ModelState.AddModelError(string.Empty, "Please correct the errors below and try again.");
                }
            }
            catch (Exception ex)
            {
                // Catch any unexpected exceptions
                ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration. Please try again.");

                // In development, you might want to see the actual error:
                // ModelState.AddModelError(string.Empty, $"Debug: {ex.Message}");
            }

            return View(model);
        }


        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return RedirectToLocal(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    return RedirectToAction(nameof(Lockout));
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Lockout()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                throw new System.InvalidOperationException($"Error confirming email for user with ID '{userId}':");
            }

            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;

                // Handle profile picture upload if provided
                if (model.ProfilePicture != null)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/profiles");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(fileStream);
                    }

                    user.ProfilePictureUrl = $"/uploads/profiles/{fileName}";
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["StatusMessage"] = "Your profile has been updated";
                    return RedirectToAction(nameof(Profile));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }
    }
}