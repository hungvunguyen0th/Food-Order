using Asm_GD1.Models;
using Asm_GD1.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Asm_GD1.Controllers
{
    [Authorize(Roles = "AdminIT,UserAdmin")]
    public class UserAdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserAdminController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ✅ DASHBOARD
        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "Dashboard - Quản lý User";
            var totalUsers = await _userManager.Users.CountAsync();
            ViewBag.TotalUsers = totalUsers;
            return View();
        }

        // ✅ INDEX
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý người dùng";
            var users = await _userManager.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();

            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    FullName = user.FullName ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Role = roles.FirstOrDefault() ?? "Customer",
                    CreatedAt = user.CreatedAt
                });
            }

            return View(userViewModels);
        }

        // ✅ CREATE GET
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Thêm người dùng";
            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState KHÔNG HỢP LỆ:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"   - {error.ErrorMessage}");
                }
                ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return View(model);
            }

            Console.WriteLine($"✅ Đang tạo user: {model.Email}");

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
                Console.WriteLine($"✅ Tạo user thành công!");

                if (!string.IsNullOrEmpty(model.Role))
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    Console.WriteLine($"✅ Đã gán role: {model.Role}");

                    // Thêm claims
                    if (model.Role == "FoodAdmin")
                        await _userManager.AddClaimAsync(user, new Claim("CanManageFood", "true"));
                    else if (model.Role == "UserAdmin")
                        await _userManager.AddClaimAsync(user, new Claim("CanManageUser", "true"));
                    else if (model.Role == "Admin")
                    {
                        await _userManager.AddClaimAsync(user, new Claim("CanManageFood", "true"));
                        await _userManager.AddClaimAsync(user, new Claim("CanManageUser", "true"));
                    }
                }

                TempData["Success"] = "Thêm người dùng thành công!";
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("❌ LỖI TẠO USER:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"   - {error.Description}");
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }


        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                CurrentRole = roles.FirstOrDefault() ?? ""
            };

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            // ✅ BẢO VỆ SUPER ADMIN
            if (user.Email == "adminit@gmail.com" && model.NewRole != "Admin")
            {
                TempData["Error"] = "Không thể thay đổi role của Super Administrator!";
                return RedirectToAction(nameof(Index));
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.UpdatedAt = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.NewRole) && model.NewRole != model.CurrentRole)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, model.NewRole);

                    var claims = await _userManager.GetClaimsAsync(user);
                    await _userManager.RemoveClaimsAsync(user, claims);

                    if (model.NewRole == "FoodAdmin")
                        await _userManager.AddClaimAsync(user, new Claim("CanManageFood", "true"));
                    else if (model.NewRole == "UserAdmin")
                        await _userManager.AddClaimAsync(user, new Claim("CanManageUser", "true"));
                    else if (model.NewRole == "Admin")
                    {
                        await _userManager.AddClaimAsync(user, new Claim("CanManageFood", "true"));
                        await _userManager.AddClaimAsync(user, new Claim("CanManageUser", "true"));
                    }
                }

                TempData["Success"] = "Cập nhật người dùng thành công!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        // ✅ DELETE GET
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // ✅ BẢO VỆ SUPER ADMIN
            if (user.Email == "adminit@gmail.com")
            {
                TempData["Error"] = "Không thể xóa Super Administrator!";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var model = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FullName = user.FullName ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Role = roles.FirstOrDefault() ?? "",
                CreatedAt = user.CreatedAt
            };

            return View(model);
        }

        // ✅ DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // ✅ BẢO VỆ SUPER ADMIN
            if (user.Email == "adminit@gmail.com")
            {
                TempData["Error"] = "Không thể xóa Super Administrator!";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                TempData["Success"] = "Xóa người dùng thành công!";
            else
                TempData["Error"] = "Không thể xóa người dùng!";

            return RedirectToAction(nameof(Index));
        }
    }
}
