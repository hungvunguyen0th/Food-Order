using Microsoft.AspNetCore.Mvc;

namespace Asm_GD1.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Story() => View();
        public IActionResult Team() => View();
        public IActionResult Values() => View();
    }
}
