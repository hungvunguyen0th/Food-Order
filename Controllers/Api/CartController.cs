using Asm_GD1.Data;
using Asm_GD1.Models;
using Asm_GD1.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Asm_GD1.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private string GetSessionId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                return userId;
            }

            // For anonymous users, use a header-based session ID
            var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
            }
            return sessionId;
        }

        private async Task<Cart> GetOrCreateCartAsync(string sessionId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserID == sessionId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserID = sessionId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        /// <summary>
        /// Get current cart
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
        {
            var sessionId = GetSessionId();
            var cart = await GetOrCreateCartAsync(sessionId);

            var cartDto = new CartDto
            {
                CartID = cart.CartID,
                UserID = cart.UserID,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemID = ci.CartItemID,
                    ProductID = ci.ProductID,
                    ProductName = ci.ProductName,
                    ProductImage = ci.ProductImage,
                    SizeID = ci.SizeID,
                    SizeName = ci.SizeName,
                    ToppingID = ci.ToppingID,
                    ToppingName = ci.ToppingName,
                    ToppingIDs = ci.ToppingIDs,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    TotalPrice = ci.TotalPrice,
                    Note = ci.Note
                }).ToList(),
                TotalPrice = cart.CartItems.Sum(ci => ci.TotalPrice),
                TotalItems = cart.CartItems.Sum(ci => ci.Quantity),
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };

            Response.Headers.Append("X-Session-Id", sessionId);
            return Ok(ApiResponse<CartDto>.SuccessResponse(cartDto));
        }

        /// <summary>
        /// Add item to cart
        /// </summary>
        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<CartDto>>> AddToCart([FromBody] AddToCartDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<CartDto>.ErrorResponse("Invalid data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var product = await _context.Products.FindAsync(dto.ProductID);
            if (product == null)
            {
                return NotFound(ApiResponse<CartDto>.ErrorResponse("Product not found"));
            }

            var sessionId = GetSessionId();
            var cart = await GetOrCreateCartAsync(sessionId);

            var size = dto.SizeID.HasValue
                ? await _context.ProductSizes.FindAsync(dto.SizeID.Value)
                : null;

            var toppings = dto.ToppingIds != null && dto.ToppingIds.Length > 0
                ? await _context.ProductToppings
                    .Where(t => dto.ToppingIds.Contains(t.ToppingID))
                    .ToListAsync()
                : new List<ProductTopping>();

            decimal basePrice = product.DiscountPrice > 0 ? product.DiscountPrice : product.BasePrice;
            decimal sizeExtra = size?.ExtraPrice ?? 0;
            decimal toppingExtra = toppings.Sum(t => t.ExtraPrice);
            decimal unitPrice = basePrice + sizeExtra + toppingExtra;

            var cartItem = new CartItem
            {
                CartID = cart.CartID,
                ProductID = product.ProductID,
                ProductImage = product.ImageUrl,
                ProductName = product.Name,
                SizeID = size?.SizeID,
                SizeName = size?.Name ?? "",
                ToppingIDs = dto.ToppingIds != null && dto.ToppingIds.Length > 0
                    ? string.Join(",", dto.ToppingIds)
                    : "",
                ToppingName = string.Join(", ", toppings.Select(t => t.Name)),
                Note = dto.Note ?? "",
                UnitPrice = unitPrice,
                Quantity = dto.Quantity > 0 ? dto.Quantity : 1,
                Price = unitPrice
            };

            cart.CartItems.Add(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var cartDto = new CartDto
            {
                CartID = cart.CartID,
                UserID = cart.UserID,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemID = ci.CartItemID,
                    ProductID = ci.ProductID,
                    ProductName = ci.ProductName,
                    ProductImage = ci.ProductImage,
                    SizeID = ci.SizeID,
                    SizeName = ci.SizeName,
                    ToppingID = ci.ToppingID,
                    ToppingName = ci.ToppingName,
                    ToppingIDs = ci.ToppingIDs,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    TotalPrice = ci.TotalPrice,
                    Note = ci.Note
                }).ToList(),
                TotalPrice = cart.CartItems.Sum(ci => ci.TotalPrice),
                TotalItems = cart.CartItems.Sum(ci => ci.Quantity),
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };

            Response.Headers.Append("X-Session-Id", sessionId);
            return Ok(ApiResponse<CartDto>.SuccessResponse(cartDto, "Item added to cart"));
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        [HttpPut("update")]
        public async Task<ActionResult<ApiResponse<CartDto>>> UpdateCartItem([FromBody] UpdateCartItemDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<CartDto>.ErrorResponse("Invalid data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var sessionId = GetSessionId();
            var cart = await GetOrCreateCartAsync(sessionId);

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CartItemID == dto.CartItemID);
            if (cartItem == null)
            {
                return NotFound(ApiResponse<CartDto>.ErrorResponse("Cart item not found"));
            }

            if (dto.Quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = Math.Clamp(dto.Quantity, 1, 10);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Reload cart to get updated items
            cart = await GetOrCreateCartAsync(sessionId);

            var cartDto = new CartDto
            {
                CartID = cart.CartID,
                UserID = cart.UserID,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemID = ci.CartItemID,
                    ProductID = ci.ProductID,
                    ProductName = ci.ProductName,
                    ProductImage = ci.ProductImage,
                    SizeID = ci.SizeID,
                    SizeName = ci.SizeName,
                    ToppingID = ci.ToppingID,
                    ToppingName = ci.ToppingName,
                    ToppingIDs = ci.ToppingIDs,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    TotalPrice = ci.TotalPrice,
                    Note = ci.Note
                }).ToList(),
                TotalPrice = cart.CartItems.Sum(ci => ci.TotalPrice),
                TotalItems = cart.CartItems.Sum(ci => ci.Quantity),
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };

            Response.Headers.Append("X-Session-Id", sessionId);
            return Ok(ApiResponse<CartDto>.SuccessResponse(cartDto, "Cart updated"));
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        [HttpDelete("remove/{itemId}")]
        public async Task<ActionResult<ApiResponse<CartDto>>> RemoveFromCart(int itemId)
        {
            var sessionId = GetSessionId();
            var cart = await GetOrCreateCartAsync(sessionId);

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CartItemID == itemId);
            if (cartItem == null)
            {
                return NotFound(ApiResponse<CartDto>.ErrorResponse("Cart item not found"));
            }

            _context.CartItems.Remove(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Reload cart to get updated items
            cart = await GetOrCreateCartAsync(sessionId);

            var cartDto = new CartDto
            {
                CartID = cart.CartID,
                UserID = cart.UserID,
                Items = cart.CartItems.Select(ci => new CartItemDto
                {
                    CartItemID = ci.CartItemID,
                    ProductID = ci.ProductID,
                    ProductName = ci.ProductName,
                    ProductImage = ci.ProductImage,
                    SizeID = ci.SizeID,
                    SizeName = ci.SizeName,
                    ToppingID = ci.ToppingID,
                    ToppingName = ci.ToppingName,
                    ToppingIDs = ci.ToppingIDs,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    TotalPrice = ci.TotalPrice,
                    Note = ci.Note
                }).ToList(),
                TotalPrice = cart.CartItems.Sum(ci => ci.TotalPrice),
                TotalItems = cart.CartItems.Sum(ci => ci.Quantity),
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };

            Response.Headers.Append("X-Session-Id", sessionId);
            return Ok(ApiResponse<CartDto>.SuccessResponse(cartDto, "Item removed from cart"));
        }

        /// <summary>
        /// Clear all items from cart
        /// </summary>
        [HttpDelete("clear")]
        public async Task<ActionResult<ApiResponse>> ClearCart()
        {
            var sessionId = GetSessionId();
            var cart = await GetOrCreateCartAsync(sessionId);

            _context.CartItems.RemoveRange(cart.CartItems);
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            Response.Headers.Append("X-Session-Id", sessionId);
            return Ok(ApiResponse.SuccessResponse("Cart cleared"));
        }

        /// <summary>
        /// Get cart item count
        /// </summary>
        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<int>>> GetCartCount()
        {
            var sessionId = GetSessionId();
            var cart = await GetOrCreateCartAsync(sessionId);

            var count = cart.CartItems.Sum(ci => ci.Quantity);

            Response.Headers.Append("X-Session-Id", sessionId);
            return Ok(ApiResponse<int>.SuccessResponse(count));
        }
    }
}
