using Asm_GD1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Asm_GD1.Controllers
{
    public class SeedController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Truy cập: https://localhost:xxxxx/Seed/CreateStaff
        public async Task<IActionResult> CreateStaff()
        {
            var messages = new List<string>();

            // Tạo role Staff nếu chưa có
            if (!await _roleManager.RoleExistsAsync("Staff"))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Staff"));
                if (roleResult.Succeeded)
                {
                    messages.Add("✅ Đã tạo role Staff");
                }
            }
            else
            {
                messages.Add("ℹ️ Role Staff đã tồn tại");
            }

            // Tạo tài khoản Staff 1
            if (await _userManager.FindByEmailAsync("staff@gmail.com") == null)
            {
                var staff = new ApplicationUser
                {
                    UserName = "staff@gmail.com",
                    Email = "staff@gmail.com",
                    FullName = "Nhân viên bán hàng",
                    PhoneNumber = "0123456789",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(staff, "Staff@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(staff, "Staff");
                    messages.Add("✅ Đã tạo tài khoản: staff@gmail.com / Staff@123");
                }
                else
                {
                    messages.Add("❌ Lỗi tạo staff@gmail.com: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                messages.Add("ℹ️ Tài khoản staff@gmail.com đã tồn tại");
            }

            // Tạo tài khoản Staff 2
            if (await _userManager.FindByEmailAsync("staff2@gmail.com") == null)
            {
                var staff2 = new ApplicationUser
                {
                    UserName = "staff2@gmail.com",
                    Email = "staff2@gmail.com",
                    FullName = "Nguyễn Thị B - Nhân viên",
                    PhoneNumber = "0987654321",
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(staff2, "Staff@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(staff2, "Staff");
                    messages.Add("✅ Đã tạo tài khoản: staff2@gmail.com / Staff@123");
                }
                else
                {
                    messages.Add("❌ Lỗi tạo staff2@gmail.com: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                messages.Add("ℹ️ Tài khoản staff2@gmail.com đã tồn tại");
            }

            return Content(string.Join("\n", messages), "text/plain; charset=utf-8");
        }
    }
}