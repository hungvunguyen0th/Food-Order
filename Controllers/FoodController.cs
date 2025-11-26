using Asm_GD1.Data;
using Asm_GD1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asm_GD1.Controllers
{
    public class FoodController : Controller
    {
        private readonly AppDbContext _context;

        public FoodController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Action Order - GIỮ NGUYÊN
        public IActionResult Order(string category = "all")
        {
            // Dữ liệu món ăn tĩnh - lấy từ Home/Index và mở rộng
            var allMenuItems = new[]
            {
                // Món cơm
                new { Name = "Cơm tấm sườn nướng", Category = "com", Image = "/Images/com-tam.jpg", Price = 45000, Description = "Cơm tấm thơm ngon với sườn nướng BBQ, chả trứng và bì" },
                new { Name = "Cơm gà nướng mật ong", Category = "com", Image = "/Images/com-ga.jpg", Price = 55000, Description = "Cơm gà nướng mật ong thơm phức, kèm rau củ tươi ngon" },
                new { Name = "Cơm chiên dương châu", Category = "com", Image = "/Images/com-chien.jpg", Price = 40000, Description = "Cơm chiên thơm ngon với tôm, xúc xích và rau củ" },
                new { Name = "Cơm niêu Singapore", Category = "com", Image = "/Images/com-nieu.jpg", Price = 65000, Description = "Cơm niêu đặc biệt với tôm, thịt và trứng" },
                
                // Món phở
                new { Name = "Phở bò tái", Category = "pho", Image = "/Images/pho-bo.jpg", Price = 65000, Description = "Phở bò truyền thống với thịt tái tươi ngon, nước dùng đậm đà" },
                new { Name = "Phở gà", Category = "pho", Image = "/Images/pho-ga.jpg", Price = 55000, Description = "Phở gà thanh đạm với thịt gà ta thơm ngon" },
                new { Name = "Phở đặc biệt", Category = "pho", Image = "/Images/pho-dac-biet.jpg", Price = 75000, Description = "Phở đầy đủ với tái, chín, gân, sách" },
                
                // Món bánh mì
                new { Name = "Bánh mì thịt nướng", Category = "banh-mi", Image = "/Images/banh-mi-thit-nuong.jpg", Price = 25000, Description = "Bánh mì Việt Nam với thịt nướng thơm lừng, rau thơm tươi" },
                new { Name = "Bánh mì pate", Category = "banh-mi", Image = "/Images/banh-mi-pate.jpg", Price = 20000, Description = "Bánh mì pate truyền thống với rau thơm" },
                new { Name = "Bánh mì gà nướng", Category = "banh-mi", Image = "/Images/banh-mi-ga.jpg", Price = 30000, Description = "Bánh mì với gà nướng ngũ vị thơm lừng" },
                
                // Món bún
                new { Name = "Bún bò Huế", Category = "bun", Image = "/Images/bun-bo.jpg", Price = 60000, Description = "Bún bò Huế cay nồng đặc trưng, chả cua, giò heo" },
                new { Name = "Bún riêu cua", Category = "bun", Image = "/Images/bun-rieu.jpg", Price = 45000, Description = "Bún riêu cua đồng với cà chua, đậu hũ" },
                new { Name = "Bún thịt nướng", Category = "bun", Image = "/Images/bun-thit-nuong.jpg", Price = 50000, Description = "Bún thịt nướng với rau sống và nước mắm" },
                
                // Hủ tiếu
                new { Name = "Hủ tiếu Nam Vang", Category = "hu-tieu", Image = "/Images/hu-tieu-nam-vang.jpg", Price = 50000, Description = "Hủ tiếu Nam Vang truyền thống với tôm, thịt, gan, tim" },
                new { Name = "Hủ tiếu gõ", Category = "hu-tieu", Image = "/Images/hu-tiu-go.jpg", Price = 55000, Description = "Hủ tiếu gõ với tôm, cua, thịt băm" },
                
                // Đồ uống
                new { Name = "Trà sữa trân châu", Category = "do-uong", Image = "/Images/tra-sua.jpg", Price = 30000, Description = "Trà sữa thơm ngon với trân châu dai giòn" },
                new { Name = "Nước cam tươi", Category = "do-uong", Image = "/Images/nuoc-cam.jpg", Price = 15000, Description = "Nước cam tươi 100% không đường" },
                new { Name = "Cà phê sữa đá", Category = "do-uong", Image = "/Images/ca-phe.jpg", Price = 20000, Description = "Cà phê phin truyền thống với sữa đặc" },
                new { Name = "Sinh tố bơ", Category = "do-uong", Image = "/Images/sinh-to-bo.jpg", Price = 25000, Description = "Sinh tố bơ béo ngậy, thơm ngon" },
                
                // Tráng miệng
                new { Name = "Chè ba màu", Category = "trang-mieng", Image = "/Images/che-ba-mau.jpg", Price = 25000, Description = "Chè ba màu mát lạnh với đậu đỏ, thạch" },
                new { Name = "Bánh flan", Category = "trang-mieng", Image = "/Images/banh-flan.jpg", Price = 20000, Description = "Bánh flan mềm mịn, ngọt thanh" },
                new { Name = "Kem xôi", Category = "trang-mieng", Image = "/Images/kem-xoi.jpg", Price = 30000, Description = "Kem xôi truyền thống với đậu xanh" }
            };

            // Lọc theo category nếu có
            if (category != "all")
            {
                allMenuItems = allMenuItems.Where(x => x.Category == category).ToArray();
            }

            ViewBag.MenuItems = allMenuItems;
            ViewBag.CurrentCategory = category;

            return View();
        }

        // ✅ SỬA ACTION DETAIL - HỖ TRỢ CẢ ID VÀ SLUG
        public IActionResult Detail(string slug = null, int? id = null)
        {
            Product product = null;

            // ✅ Thử tìm theo Slug trước (nếu có)
            if (!string.IsNullOrEmpty(slug))
            {
                product = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Size)
                    .Include(p => p.Topping)
                    .AsNoTracking()
                    .FirstOrDefault(p => p.Slug == slug);
            }

            // ✅ Nếu không tìm thấy bằng Slug, thử tìm theo ID
            if (product == null && id.HasValue)
            {
                product = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Size)
                    .Include(p => p.Topping)
                    .AsNoTracking()
                    .FirstOrDefault(p => p.ProductID == id.Value);
            }

            // ✅ Nếu vẫn không tìm thấy, tạo product tĩnh
            if (product == null)
            {
                // Tạo product mặc định từ dữ liệu tĩnh
                product = new Product
                {
                    ProductID = id ?? 1,
                    Name = "Cơm tấm sườn nướng",
                    BasePrice = 45000,
                    DiscountPercent = 15,
                    ImageUrl = "/Images/com-tam.jpg",
                    Description = "Cơm tấm thơm ngon với sườn nướng BBQ, chả trứng và bì",
                    Rating = 4.8m,
                    Category = new Category { CategoryID = 1, Name = "Món cơm" }
                };
            }

            // ✅ Load sizes và toppings
            var sizes = _context.ProductSizes.AsNoTracking().OrderBy(s => s.ExtraPrice).ToList();
            var toppings = _context.ProductToppings.AsNoTracking().ToList();

            // ✅ Nếu không có sizes/toppings trong DB, tạo dữ liệu tĩnh
            if (!sizes.Any())
            {
                sizes = new List<ProductSize>
                {
                    new ProductSize { SizeID = 1, Name = "Nhỏ", ExtraPrice = 0 },
                    new ProductSize { SizeID = 2, Name = "Vừa", ExtraPrice = 5000 },
                    new ProductSize { SizeID = 3, Name = "Lớn", ExtraPrice = 10000 }
                };
            }

            if (!toppings.Any())
            {
                toppings = new List<ProductTopping>
                {
                    new ProductTopping { ToppingID = 1, Name = "Trứng", ExtraPrice = 5000 },
                    new ProductTopping { ToppingID = 2, Name = "Phô mai", ExtraPrice = 8000 },
                    new ProductTopping { ToppingID = 3, Name = "Thịt thêm", ExtraPrice = 15000 }
                };
            }

            // ✅ Tạo ViewModel
            var vm = new ProductViewModel
            {
                Product = new List<Product> { product },
                Sizes = sizes,
                Toppings = toppings
            };

            return View(vm);
        }

        // ✅ Action Category - GIỮ NGUYÊN
        public IActionResult Category(string type) => View();
    }
}
