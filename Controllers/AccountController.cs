using Asm_GD1.Data;
using Asm_GD1.Models;
using Asm_GD1.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asm_GD1.Controllers
{
    public class AccountController : BaseController
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            AppDbContext context,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager) : base(context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Login() => View();
        public IActionResult Register() => View();
        public IActionResult Profile() => View();
        public IActionResult Orders() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Customer");

                await _signInManager.SignInAsync(user, isPersistent: false);

                var cart = await GetOrCreateActiveCartAsync(user.Id);
                SetCartIdToSession(cart.CartID);

                TempData["SuccessMessage"] = "Đăng ký thành công!";
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var cart = await GetOrCreateActiveCartAsync(user.Id);
                    SetCartIdToSession(cart.CartID);

                    var roles = await _userManager.GetRolesAsync(user);
                    var role = roles.FirstOrDefault()?.ToLower() ?? "customer";

                    TempData["SuccessMessage"] = $"Chào mừng {user.FullName ?? user.Email}!";

                    if (role == "admin")
                    {
                        return RedirectToAction("Dashboard", "FoodAdmin");
                    }
                    else if (role == "foodadmin")
                    {
                        return RedirectToAction("Dashboard", "FoodAdmin");
                    }
                    else if (role == "useradmin")
                    {
                        return RedirectToAction("Dashboard", "UserAdmin");
                    }
                    else if (role == "staff")
                    {
                        return RedirectToAction("Index", "POS");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ViewBag.Error = "Email hoặc mật khẩu không đúng.";
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Remove(SessionCartIdKey);
            TempData["SuccessMessage"] = "Đăng xuất thành công.";
            return RedirectToAction("Login");
        }
    }
}
