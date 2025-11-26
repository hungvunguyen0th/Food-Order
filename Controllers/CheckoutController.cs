using Microsoft.AspNetCore.Mvc;

namespace Asm_GD1.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Success() => View();
        public IActionResult Failed() => View();
    }
}
