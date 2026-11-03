using BusinessManagementSystem.ViewModels;
using BusinessManagementSystem.Data;
using BusinessManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BusinessManagementSystem.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> Index(string? status)
        {
            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Service)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            ViewBag.Status = status;

            return View(await orders
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync());
        }

        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.ApplicationUserId == userId &&
                    c.IsActive);

            if (customer == null)
                return NotFound();

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Service)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .Where(o => o.CustomerId == customer.CustomerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }


        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpGet]
        public async Task<IActionResult> Create(int? serviceRequestId)
        {
            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.FullName)
                .ToListAsync();

            ViewBag.Customers = customers;

            ViewBag.Products = await _context.Products
    .Where(p => p.IsActive)
    .OrderBy(p => p.Name)
    .ToListAsync();

            ViewBag.Services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var model = new CreateOrderViewModel();

            if (serviceRequestId.HasValue)
            {
                var request = await _context.ServiceRequests
                    .FirstOrDefaultAsync(r =>
                        r.ServiceRequestId == serviceRequestId.Value &&
                        r.Status == "Approved");

                if (request != null)
                {
                    model.CustomerId = request.CustomerId;
                    model.ItemType = "Service";
                    model.ItemId = request.ServiceId;
                }
            }

            return View(model);
        }



        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            var customerExists = await _context.Customers
                .AnyAsync(c => c.CustomerId == model.CustomerId && c.IsActive);

            if (!customerExists)
                ModelState.AddModelError(nameof(model.CustomerId), "Select a valid customer.");

            if (model.ItemType == "Product")
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == model.ItemId && p.IsActive);

                if (product == null)
                    ModelState.AddModelError(nameof(model.ItemId), "Select a valid product.");
                else if (model.Quantity > product.StockQuantity)
                    ModelState.AddModelError(nameof(model.Quantity), "Quantity exceeds available stock.");
            }
            else if (model.ItemType == "Service")
            {
                var serviceExists = await _context.Services
                    .AnyAsync(s => s.ServiceId == model.ItemId && s.IsActive);

                if (!serviceExists)
                    ModelState.AddModelError(nameof(model.ItemId), "Select a valid service.");
            }
            else
            {
                ModelState.AddModelError(nameof(model.ItemType), "Select Product or Service.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Customers = await _context.Customers
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.FullName)
                    .ToListAsync();

                ViewBag.Products = await _context.Products
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                ViewBag.Services = await _context.Services
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                return View("Create", model);
            }

            var order = new Order
            {
                CustomerId = model.CustomerId,
                Status = "Pending",
                OrderDate = DateTime.UtcNow
            };

            if (model.ItemType == "Product")
            {
                var product = await _context.Products
                    .FirstAsync(p => p.ProductId == model.ItemId && p.IsActive);

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.ProductId,
                    Quantity = model.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = product.Price * model.Quantity
                });

                product.StockQuantity -= model.Quantity;
            }
            else
            {
                var service = await _context.Services
                    .FirstAsync(s => s.ServiceId == model.ItemId && s.IsActive);

                order.OrderItems.Add(new OrderItem
                {
                    ServiceId = service.ServiceId,
                    Quantity = model.Quantity,
                    UnitPrice = service.Price,
                    TotalPrice = service.Price * model.Quantity
                });
            }

            order.TotalAmount = order.OrderItems.Sum(i => i.TotalPrice);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order created successfully.";
            return RedirectToAction(nameof(Index));
        }



        [Authorize(Roles = "Admin,Manager,Staff")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var allowedStatuses = new[]
            {
        "Pending",
        "Confirmed",
        "Processing",
        "Completed",
        "Cancelled"
    };

            if (!allowedStatuses.Contains(status))
                return BadRequest();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            if (order.Status == "Cancelled" && status != "Cancelled")
            {
                TempData["ErrorMessage"] =
                    "A cancelled order cannot be reopened.";

                return RedirectToAction(nameof(Index));
            }

            if (order.Status == "Completed" && status != "Completed")
            {
                TempData["ErrorMessage"] =
                    "A completed order's status cannot be changed.";

                return RedirectToAction(nameof(Index));
            }

            if (status == "Cancelled" &&
                !User.IsInRole("Admin") &&
                !User.IsInRole("Manager"))
            {
                return Forbid();
            }

            // Step 1: Check payments BEFORE restoring stock.
            if (status == "Cancelled")
            {
                var invoice = await _context.Invoices
                    .FirstOrDefaultAsync(i => i.OrderId == id);

                if (invoice != null)
                {
                    var hasCompletedPayments = await _context.Payments
                        .AnyAsync(p =>
                            p.InvoiceId == invoice.InvoiceId &&
                            p.Status == "Completed");

                    if (hasCompletedPayments)
                    {
                        TempData["ErrorMessage"] =
                            "This order has an invoice with recorded payments. Reverse or refund the payments before cancelling the order.";

                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            // Step 2: Restore stock only after payment validation passes.
            if (status == "Cancelled" && order.Status != "Cancelled")
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.StockQuantity += item.Quantity;
                    }
                }
            }

            // Step 3: Update status and save.
            order.Status = status;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order status updated.";

            return RedirectToAction(nameof(Index));
        }


    }
}