using Microsoft.AspNetCore.Mvc;

namespace Asm_GD1.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Detail(int id) => View();
    }
}
