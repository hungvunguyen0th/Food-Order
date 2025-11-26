using Asm_GD1.Data;
using Asm_GD1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asm_GD1.Controllers
{
    [Authorize(Policy = "CanManageFood")]
    public class DiscountAdminController : Controller
    {
        private readonly AppDbContext _context;

        public DiscountAdminController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ INDEX - DANH SÁCH MÃ GIẢM GIÁ
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý Mã giảm giá";
            var discounts = await _context.Discounts
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return View(discounts);
        }

        // ✅ CREATE GET
        public IActionResult Create()
        {
            return View();
        }

        // ✅ CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Discount discount)
        {
            if (!ModelState.IsValid)
            {
                return View(discount);
            }

            // Kiểm tra mã giảm giá đã tồn tại chưa
            var existingDiscount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.Code == discount.Code);

            if (existingDiscount != null)
            {
                ModelState.AddModelError("Code", "Mã giảm giá đã tồn tại!");
                return View(discount);
            }

            // Kiểm tra ngày bắt đầu phải nhỏ hơn ngày kết thúc
            if (discount.StartDate >= discount.EndDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải lớn hơn ngày bắt đầu!");
                return View(discount);
            }

            discount.CreatedAt = DateTime.Now;
            discount.UpdatedAt = DateTime.Now;
            discount.IsActive = true;

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm mã giảm giá thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ EDIT GET
        public async Task<IActionResult> Edit(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            return View(discount);
        }

        // ✅ EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Discount discount)
        {
            if (id != discount.DiscountID) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(discount);
            }

            // Kiểm tra ngày
            if (discount.StartDate >= discount.EndDate)
            {
                ModelState.AddModelError("EndDate", "Ngày kết thúc phải lớn hơn ngày bắt đầu!");
                return View(discount);
            }

            var existingDiscount = await _context.Discounts.FindAsync(id);
            if (existingDiscount == null) return NotFound();

            existingDiscount.Code = discount.Code;
            existingDiscount.Description = discount.Description;
            existingDiscount.DiscountPercent = discount.DiscountPercent;
            existingDiscount.MaxDiscountAmount = discount.MaxDiscountAmount;
            existingDiscount.MinOrderAmount = discount.MinOrderAmount;
            existingDiscount.StartDate = discount.StartDate;
            existingDiscount.EndDate = discount.EndDate;
            existingDiscount.UsageLimit = discount.UsageLimit;
            existingDiscount.IsActive = discount.IsActive;
            existingDiscount.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật mã giảm giá thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ DELETE GET
        public async Task<IActionResult> Delete(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            return View(discount);
        }

        // ✅ DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa mã giảm giá thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ TOGGLE STATUS
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            discount.IsActive = !discount.IsActive;
            discount.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = discount.IsActive });
        }
    }
}
