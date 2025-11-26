using Microsoft.AspNetCore.Mvc;

namespace Asm_GD1.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult SendMessage(string name, string email, string phone, string subject, string message)
        {
            // Xử lý gửi tin nhắn (tạm thời chỉ trả về thông báo thành công)
            TempData["ContactSuccess"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi trong 24h.";
            return RedirectToAction("Index");
        }
    }
}
