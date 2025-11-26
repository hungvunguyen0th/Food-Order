using Asm_GD1.Data;
using Asm_GD1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Asm_GD1.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly AppDbContext _context;
        protected const string SessionCartIdKey = "CartId";

        public BaseController(AppDbContext context)
        {
            _context = context;
        }

        protected int? GetCartIdFromSession()
        {
            return HttpContext.Session.GetInt32(SessionCartIdKey);
        }

        protected void SetCartIdToSession(int cartId)
        {
            HttpContext.Session.SetInt32(SessionCartIdKey, cartId);
        }

        protected async Task<Cart> GetOrCreateActiveCartAsync(string? userId)
        {
            var cartIdString = HttpContext.Session.GetString("CartID");
            int? cartId = null;

            if (!string.IsNullOrEmpty(cartIdString) && int.TryParse(cartIdString, out int parsedCartId))
            {
                cartId = parsedCartId;

                var existingCart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.CartID == cartId.Value);

                if (existingCart != null)
                {
                    return existingCart;
                }
            }

            var newCart = new Cart
            {
                UserID = userId ?? User.Identity?.Name,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.Carts.Add(newCart);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("CartID", newCart.CartID.ToString());

            return newCart;
        }


        protected async Task<Cart> GetCartAsync(string? userId)
        {
            var cartId = GetCartIdFromSession();
            if (cartId.HasValue)
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.CartID == cartId && c.UserID == userId);

                if (cart != null) return cart;
            }

            var newCart = await GetOrCreateActiveCartAsync(userId);
            SetCartIdToSession(newCart.CartID);
            return newCart;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var cartIdString = HttpContext.Session.GetString("CartID");
            ViewBag.CartItemCount = 0;

            if (!string.IsNullOrEmpty(cartIdString) && int.TryParse(cartIdString, out int cartId))
            {
                ViewBag.CartItemCount = _context.CartItems
                    .Where(ci => ci.CartID == cartId)
                    .Sum(ci => (int?)ci.Quantity) ?? 0;
            }
        }
    }
}
