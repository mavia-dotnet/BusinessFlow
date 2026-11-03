using BusinessManagementSystem.Data;
using BusinessManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BusinessManagementSystem.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.ApplicationUserId == userId &&
                    c.IsActive);

            if (customer == null)
            {
                return NotFound();
            }

            var model = new CustomerProfileViewModel
            {
                FullName = customer.FullName,
                Email = customer.Email ?? string.Empty,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CustomerProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.ApplicationUserId == userId &&
                    c.IsActive);

            if (customer == null)
            {
                return NotFound();
            }

            customer.FullName = model.FullName;
            customer.Email = model.Email;
            customer.Phone = model.Phone;
            customer.Address = model.Address;
            customer.City = model.City;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Profile updated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}