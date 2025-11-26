using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asm_GD1.Controllers
{
    [Authorize(Roles = "staff,employee1,employee2")]
    public class SalesStaffController : Controller
    {
        // Dashboard chính của nhân viên bán hàng
        public IActionResult Dashboard()
        {
            // Dữ liệu mẫu cho dashboard
            ViewBag.TodayOrders = 15;
            ViewBag.TodayRevenue = 2850000;
            ViewBag.PendingOrders = 3;
            ViewBag.CustomersServed = 12;

            // Danh sách đơn hàng đang chờ (mô phỏng đơn hàng tại chỗ như KFC)
            var orders = new List<dynamic>
    {
        new {
            Id = "ORD001",
            CustomerName = "Nguyễn Văn A",
            Phone = "0123456789",
            Items = "2x Gà rán, 1x Pepsi",
            Amount = 125000,
            Address = "Ăn tại chỗ - Bàn số 5",
            Time = DateTime.Now.AddMinutes(-8),
            Priority = "high"
        },
        new {
            Id = "ORD002",
            CustomerName = "Trần Thị B",
            Phone = "0987654321",
            Items = "1x Burger gà, 1x Khoai tây chiên",
            Amount = 89000,
            Address = "Giao hàng - 456 Lê Lợi, Q1",
            Time = DateTime.Now.AddMinutes(-12),
            Priority = "normal"
        },
        new {
            Id = "ORD003",
            CustomerName = "Lê Văn C",
            Phone = "0369852147",
            Items = "3x Gà rán cay, 2x Coca Cola",
            Amount = 156000,
            Address = "Ăn tại chỗ - Bàn số 12",
            Time = DateTime.Now.AddMinutes(-15),
            Priority = "high"
        }
    };

            ViewBag.Orders = orders;
            return View();
        }

        // Trang tạo đơn hàng mới
        public IActionResult CreateOrder()
        {
            // Menu items mẫu (như KFC)
            var menuItems = new List<dynamic>
    {
        new {
            Id = 1,
            Name = "Gà rán truyền thống",
            Price = 45000,
            Category = "Gà rán",
            Available = true
        },
        new {
            Id = 2,
            Name = "Gà rán cay",
            Price = 48000,
            Category = "Gà rán",
            Available = true
        },
        new {
            Id = 3,
            Name = "Burger gà",
            Price = 55000,
            Category = "Burger",
            Available = true
        },
        new {
            Id = 4,
            Name = "Khoai tây chiên",
            Price = 25000,
            Category = "Phụ",
            Available = true
        },
        new {
            Id = 5,
            Name = "Pepsi",
            Price = 18000,
            Category = "Đồ uống",
            Available = true
        },
        new {
            Id = 6,
            Name = "Coca Cola",
            Price = 18000,
            Category = "Đồ uống",
            Available = false
        }
    };

            ViewBag.MenuItems = menuItems;
            return View();
        }

        // Xử lý tạo đơn hàng (POST)
        [HttpPost]
        public IActionResult CreateOrder(string customerName, string customerPhone, string customerAddress, string orderItems, decimal totalAmount)
        {
            // Validate dữ liệu
            if (string.IsNullOrEmpty(customerName) || string.IsNullOrEmpty(customerPhone) || totalAmount <= 0)
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin" });
            }

            // Tạo mã đơn hàng mới
            var orderId = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss");

            // Ở đây bạn sẽ lưu vào database
            // Hiện tại chỉ mô phỏng thành công

            return Json(new
            {
                success = true,
                message = "Tạo đơn hàng thành công",
                orderId = orderId,
                redirectUrl = Url.Action("Dashboard", "SalesStaff")
            });
        }
    }
}
