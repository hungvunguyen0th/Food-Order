using Asm_GD1.Data;
using Asm_GD1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Asm_GD1.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var products = _context.Products.AsNoTracking().ToList();
            var sizes = _context.ProductSizes.AsNoTracking().OrderBy(s => s.ExtraPrice).ToList();
                
            var vm = new ProductViewModel
            {
                Product = products,
                Sizes = sizes,
            };

            return View(vm);
        }
    }
}
