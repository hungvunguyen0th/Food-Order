using Asm_GD1.Data;
using Asm_GD1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asm_GD1.Controllers
{
    //[Authorize(Policy = "CanManageFood")]
    [Authorize(Roles = "AdminIT,FoodAdmin")]
    public class FoodAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public FoodAdminController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ✅ DASHBOARD
        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "Dashboard - Quản lý Thực đơn";

            var totalMenuItems = await _context.Products.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var activeDiscounts = await _context.Discounts
                .Where(d => d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now)
                .CountAsync();

            ViewBag.TotalMenuItems = totalMenuItems;
            ViewBag.TotalCategories = totalCategories;
            ViewBag.ActiveDiscounts = activeDiscounts;

            return View();
        }

        // ✅ MENU MANAGEMENT - INDEX
        public async Task<IActionResult> MenuManagement()
        {
            ViewData["Title"] = "Quản lý Thực đơn";
            var products = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(products);
        }

        // ✅ CREATE GET
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // ✅ CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }

            // Upload hình ảnh
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "Images");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                product.ImageUrl = "/Images/" + uniqueFileName;
            }

            product.CreatedAt = DateTime.Now;
            product.UpdatedAt = DateTime.Now;
            product.IsAvailable = true;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm món ăn thành công!";
            return RedirectToAction(nameof(MenuManagement));
        }

        // ✅ EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            return View(product);
        }

        // ✅ EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? ImageFile)
        {
            if (id != product.ProductID) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _context.Categories.ToList();
                return View(product);
            }

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return NotFound();

            // Upload hình ảnh mới nếu có
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "Images");
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath, existingProduct.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                existingProduct.ImageUrl = "/Images/" + uniqueFileName;
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.BasePrice = product.BasePrice;
            existingProduct.CategoryID = product.CategoryID;
            existingProduct.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật món ăn thành công!";
            return RedirectToAction(nameof(MenuManagement));
        }

        // ✅ DELETE GET
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // ✅ DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Xóa ảnh
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var imagePath = Path.Combine(_environment.WebRootPath, product.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa món ăn thành công!";
            return RedirectToAction(nameof(MenuManagement));
        }
    }
}
