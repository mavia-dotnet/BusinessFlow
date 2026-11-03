using BusinessManagementSystem.Data;
using BusinessManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Staff")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Customer
        public async Task<IActionResult> Index(string? search)
        {
            var customers = _context.Customers
                .Where(c => c.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                customers = customers.Where(c =>
                    c.FullName.Contains(search) ||
                    (c.Email != null && c.Email.Contains(search)) ||
                    (c.Phone != null && c.Phone.Contains(search)));
            }

            var customerList = await customers
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;

            return View(customerList);
        }

        // GET: /Customer/Create
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Customer/Create
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            customer.CreatedAt = DateTime.UtcNow;
            customer.IsActive = true;

            _context.Customers.Add(customer);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Customer/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // POST: /Customer/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            var existingCustomer = await _context.Customers
                .FindAsync(id);

            if (existingCustomer == null)
            {
                return NotFound();
            }

            existingCustomer.FullName = customer.FullName;
            existingCustomer.Email = customer.Email;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Address = customer.Address;
            existingCustomer.City = customer.City;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Customer/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            customer.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Customer/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.CustomerId == id &&
                    c.IsActive);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }
    }
}