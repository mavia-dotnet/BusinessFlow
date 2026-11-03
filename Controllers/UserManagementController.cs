
using BusinessManagementSystem.Models;
using BusinessManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManagementController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var staffUsers = new List<UserManagementListItem>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Manager") || roles.Contains("Staff"))
                {
                    staffUsers.Add(new UserManagementListItem
                    {
                        FullName = user.FullName,
                        Email = user.Email ?? "",
                        Role = string.Join(", ", roles),
                        UserId = user.Id
                    });
                }
            }

            return View(staffUsers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateStaffUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateStaffUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = model.Email.Trim();

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "An account with this email already exists.");

                return View(model);
            }

            if (model.Role != "Manager" && model.Role != "Staff")
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Only Manager or Staff accounts can be created here.");

                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = model.FullName.Trim(),
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(
                user, model.Password);

            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            var roleResult = await _userManager.AddToRoleAsync(
                user, model.Role);

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            TempData["SuccessMessage"] =
                $"{model.Role} account created successfully.";

            return RedirectToAction(nameof(Index));
        }
    }

    public class UserManagementListItem
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
