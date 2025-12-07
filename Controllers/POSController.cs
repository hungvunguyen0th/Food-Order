using Asm_GD1.Data;
using Asm_GD1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asm_GD1.Controllers
{
    [Authorize(Roles = "AdminIT,FoodAdmin,Staff")]
    public class POSController : Controller
    {
        private readonly AppDbContext _context;

        public POSController(AppDbContext context)
        {
            _context = context;
        }

        // ========================================
        // INDEX - TRANG POS CHÍNH
        // ========================================
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();

            if (!products.Any())
            {
                products = GetSampleProducts();
            }

            ViewBag.Categories = products.Select(p => p.Category?.Name ?? "Khác")
                .Distinct()
                .ToList();

            return View(products);
        }

        // ========================================
        // TẠO ĐƠN HÀNG
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateOrder(string customerName, string customerPhone, string orderItems, decimal totalAmount, string paymentMethod)
        {
            try
            {
                TempData["SuccessMessage"] = $"Đã tạo đơn hàng cho {customerName}.  Tổng tiền: {totalAmount:N0}₫";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi khi tạo đơn hàng: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // ========================================
        // DỮ LIỆU MẪU
        // ========================================
        private List<Product> GetSampleProducts()
        {
            return new List<Product>
            {
                new Product { ProductID = 1, Name = "Cơm tấm sườn nướng", BasePrice = 45000, ImageUrl = "/Images/com-tam. jpg", Category = new Category { Name = "Cơm" } },
                new Product { ProductID = 2, Name = "Phở bò tái", BasePrice = 65000, ImageUrl = "/Images/pho-bo. jpg", Category = new Category { Name = "Phở" } },
                new Product { ProductID = 3, Name = "Bánh mì thịt nướng", BasePrice = 25000, ImageUrl = "/Images/banh-mi-thit-nuong.jpg", Category = new Category { Name = "Bánh mì" } },
                new Product { ProductID = 4, Name = "Bún bò Huế", BasePrice = 60000, ImageUrl = "/Images/bun-bo. jpg", Category = new Category { Name = "Bún" } },
                new Product { ProductID = 5, Name = "Trà sữa trân châu", BasePrice = 30000, ImageUrl = "/Images/tra-sua. jpg", Category = new Category { Name = "Đồ uống" } },
                new Product { ProductID = 6, Name = "Cà phê sữa đá", BasePrice = 20000, ImageUrl = "/Images/ca-phe. jpg", Category = new Category { Name = "Đồ uống" } },
                new Product { ProductID = 7, Name = "Gà rán", BasePrice = 35000, ImageUrl = "/Images/ga-ran.jpg", Category = new Category { Name = "Fast Food" } },
                new Product { ProductID = 8, Name = "Burger bò phô mai", BasePrice = 45000, ImageUrl = "/Images/burger. jpg", Category = new Category { Name = "Fast Food" } }
            };
        }
    }
}