using Asm_GD1.Data;
using Asm_GD1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asm_GD1.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // ========================================
        // HELPER
        // ========================================
        private async Task<Cart> GetOrCreateCartAsync()
        {
            string userId = HttpContext.Session.GetString("CartSessionId");
            if (string.IsNullOrEmpty(userId))
            {
                userId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("CartSessionId", userId);
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserID == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserID = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        // ========================================
        // INDEX
        // ========================================
        public async Task<IActionResult> Index()
        {
            var cart = await GetOrCreateCartAsync();
            return View(cart.CartItems.ToList());
        }

        // ========================================
        // CHECKOUT
        // ========================================
        public async Task<IActionResult> Checkout()
        {
            var cart = await GetOrCreateCartAsync();
            if (!cart.CartItems.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }
            return View(cart.CartItems.ToList());
        }

        // ========================================
        // PLACE ORDER - XỬ LÝ KHI BUTTON ĐƯỢC BẤM
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(
    string fullName,
    string phone,
    string email,
    string address,
    string city,
    string district,
    string ward,
    string note,
    string deliveryTime,
    string paymentMethod)
        {
            try
            {
                Console.WriteLine("====================================");
                Console.WriteLine("[PlaceOrder] ĐƯỢC GỌI!");
                Console.WriteLine($"[PlaceOrder] fullName: {fullName}");
                Console.WriteLine($"[PlaceOrder] phone: {phone}");
                Console.WriteLine("====================================");

                var cart = await GetOrCreateCartAsync();

                if (!cart.CartItems.Any())
                {
                    Console.WriteLine("[PlaceOrder] Giỏ hàng trống!");
                    TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index");
                }

                // Tính tổng
                var subtotal = cart.CartItems.Sum(x => x.UnitPrice * x.Quantity);
                var shippingFee = deliveryTime == "delivery" ? 30000 : 0;
                var total = subtotal + shippingFee;

                Console.WriteLine($"[PlaceOrder] Subtotal: {subtotal}");
                Console.WriteLine($"[PlaceOrder] Total: {total}");

                // Xóa cart items
                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();

                Console.WriteLine("[PlaceOrder] CartItems đã xóa!");

                // ✅ LÀM TRÒN DECIMAL → STRING ĐỂ LƯU VÀO TEMPDATA
                TempData["OrderSuccess"] = true;
                TempData["CustomerName"] = fullName ?? "Khách hàng";
                TempData["CustomerPhone"] = phone ?? "";
                TempData["CustomerEmail"] = email ?? "";

                // Ghép địa chỉ
                string fullAddress = "Không có địa chỉ";
                if (!string.IsNullOrEmpty(address))
                {
                    fullAddress = $"{address}";
                    if (!string.IsNullOrEmpty(ward)) fullAddress += $", {ward}";
                    if (!string.IsNullOrEmpty(district)) fullAddress += $", {district}";
                    if (!string.IsNullOrEmpty(city)) fullAddress += $", {city}";
                }
                TempData["CustomerAddress"] = fullAddress;

                TempData["DeliveryType"] = deliveryTime == "now" ? "Tại chỗ" : "Giao hàng";

                string paymentName = paymentMethod switch
                {
                    "cod" => "Tiền mặt",
                    "momo" => "MoMo",
                    "zalopay" => "ZaloPay",
                    "vnpay" => "VNPAY",
                    _ => "Tiền mặt"
                };
                TempData["PaymentMethod"] = paymentName;
                TempData["Note"] = note ?? "";

                // ✅ CHUYỂN DECIMAL → STRING
                TempData["Subtotal"] = subtotal.ToString("N0");
                TempData["ShippingFee"] = shippingFee.ToString("N0");
                TempData["Total"] = total.ToString("N0");

                Console.WriteLine("[PlaceOrder] Chuyển hướng tới Success!");

                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlaceOrder] LỖI: {ex.Message}");
                Console.WriteLine($"[PlaceOrder] StackTrace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đặt hàng!";
                return RedirectToAction("Checkout");
            }
        }


        // ========================================
        // SUCCESS - TRANG HOÀN TẤT
        // ========================================
        public IActionResult Success()
        {
            Console.WriteLine("[Success] ĐƯỢC GỌI!");
            Console.WriteLine($"[Success] OrderSuccess = {TempData["OrderSuccess"]}");

            if (TempData["OrderSuccess"] == null || !(bool)TempData["OrderSuccess"])
            {
                Console.WriteLine("[Success] Không có order, chuyển về Home");
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // ========================================
        // ADD TO CART
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(
            int id,
            int? sizeId,
            int[]? toppingIds,
            int quantity,
            string note)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null) return NotFound();

                var size = sizeId.HasValue
                    ? await _context.ProductSizes.FindAsync(sizeId.Value)
                    : null;

                var toppings = toppingIds != null && toppingIds.Length > 0
                    ? await _context.ProductToppings
                        .Where(t => toppingIds.Contains(t.ToppingID))
                        .ToListAsync()
                    : new List<ProductTopping>();

                decimal basePrice = product.DiscountPrice > 0
                    ? product.DiscountPrice
                    : product.BasePrice;
                decimal sizeExtra = size?.ExtraPrice ?? 0;
                decimal toppingExtra = toppings.Sum(t => t.ExtraPrice);
                decimal unitPrice = basePrice + sizeExtra + toppingExtra;

                var cart = await GetOrCreateCartAsync();

                var newItem = new CartItem
                {
                    CartID = cart.CartID,
                    ProductID = product.ProductID,
                    ProductImage = product.ImageUrl,
                    ProductName = product.Name,
                    SizeID = size?.SizeID,
                    SizeName = size?.Name ?? "",
                    ToppingIDs = toppingIds != null && toppingIds.Length > 0
                        ? string.Join(",", toppingIds)
                        : "",
                    ToppingName = string.Join(", ", toppings.Select(t => t.Name)),
                    Note = note ?? "",
                    UnitPrice = unitPrice,
                    Quantity = quantity,
                    Price = unitPrice
                };

                cart.CartItems.Add(newItem);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã thêm món vào giỏ hàng!";
                return RedirectToAction("Index", "Cart");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra!";
                return RedirectToAction("Detail", "Food", new { id });
            }
        }

        // ========================================
        // CHANGE QUANTITY
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeQuantity(
            int productId, int? sizeId, string toppingIds, int delta)
        {
            var cart = await GetOrCreateCartAsync();
            var item = cart.CartItems.FirstOrDefault(i =>
                i.ProductID == productId
                && i.SizeID == sizeId
                && i.ToppingIDs == toppingIds);

            if (item != null)
            {
                item.Quantity = Math.Clamp(item.Quantity + delta, 1, 10);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // ========================================
        // CART COUNT VALUE - API ĐẾM SỐ LƯỢNG
        // ========================================
        [HttpGet]
        public async Task<IActionResult> CartCountValue()
        {
            try
            {
                var cart = await GetOrCreateCartAsync();
                var totalItems = cart.CartItems.Sum(x => x.Quantity);
                return Content(totalItems.ToString());
            }
            catch
            {
                return Content("0");
            }
        }
        // ========================================
        // REMOVE FROM CART - SỬA LẠI
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(
            int productId, int? sizeId, string toppingIds)
        {
            try
            {
                var cart = await GetOrCreateCartAsync();

                CartItem item = null;

                if (productId > 0)
                {
                    item = cart.CartItems.FirstOrDefault(i =>
                        i.ProductID == productId
                        && i.SizeID == sizeId
                        && i.ToppingIDs == toppingIds);
                }
                else
                {
                    var productName = Request.Form["productName"].ToString();
                    item = cart.CartItems.FirstOrDefault(i =>
                        i.ProductID == 0 &&
                        i.ProductName == productName &&
                        string.IsNullOrEmpty(i.SizeName) &&
                        string.IsNullOrEmpty(i.ToppingName));
                }

                if (item != null)
                {
                    _context.CartItems.Remove(item);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã xóa món khỏi giỏ hàng!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy món trong giỏ hàng! ";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing item: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi khi xóa món! ";
                return RedirectToAction("Index");
            }
        }

        // ========================================
        // CLEAR CART
        // ========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            var cart = await GetOrCreateCartAsync();
            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
