using BusinessManagementSystem.Data;
using BusinessManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusinessManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Staff")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Product
        public async Task<IActionResult> Index(string? search)
        {
            var products = _context.Products
                .Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p =>
                    p.Name.Contains(search) ||
                    (p.Category != null && p.Category.Contains(search)) ||
                    p.Type.Contains(search));
            }

            var productList = await products
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.Search = search;

            return View(productList);
        }
        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            product.CreatedAt = DateTime.UtcNow;
            product.IsActive = true;

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product added successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Product/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p =>
                    p.ProductId == id &&
                    p.IsActive);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: /Product/Edit/5

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FindAsync(id);

            if (product == null || !product.IsActive)
            {
                return NotFound();
            }

            return View(product);
        }


        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            var existingProduct = await _context.Products
                .FindAsync(id);

            if (existingProduct == null || !existingProduct.IsActive)
            {
                return NotFound();
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Type = product.Type;
            existingProduct.Category = product.Category;
            existingProduct.Price = product.Price;
            existingProduct.CostPrice = product.CostPrice;
            existingProduct.StockQuantity = product.StockQuantity;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST: /Product/Delete/5
        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            product.IsActive = false;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}