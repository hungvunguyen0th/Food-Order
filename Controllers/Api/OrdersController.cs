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
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private string GetSessionId()
        {
            var userId = GetUserId();
            if (!string.IsNullOrEmpty(userId))
            {
                return userId;
            }
            return Request.Headers["X-Session-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Get all orders (Admin/Staff only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "AdminIT,FoodAdmin,Staff")]
        public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderDto
                {
                    OrderID = o.OrderID,
                    UserID = o.UserID,
                    CustomerName = o.CustomerName,
                    CustomerPhone = o.CustomerPhone,
                    CustomerEmail = o.CustomerEmail,
                    ShippingAddress = o.ShippingAddress,
                    Note = o.Note,
                    Status = o.Status,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    SubTotal = o.SubTotal,
                    ShippingFee = o.ShippingFee,
                    DiscountAmount = o.DiscountAmount,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        OrderItemID = oi.OrderItemID,
                        ProductID = oi.ProductID,
                        ProductName = oi.ProductName,
                        ProductImage = oi.ProductImage,
                        SizeName = oi.SizeName,
                        ToppingName = oi.ToppingName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Note = oi.Note
                    }).ToList(),
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orders));
        }

        /// <summary>
        /// Get order by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .Where(o => o.OrderID == id)
                .Select(o => new OrderDto
                {
                    OrderID = o.OrderID,
                    UserID = o.UserID,
                    CustomerName = o.CustomerName,
                    CustomerPhone = o.CustomerPhone,
                    CustomerEmail = o.CustomerEmail,
                    ShippingAddress = o.ShippingAddress,
                    Note = o.Note,
                    Status = o.Status,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    SubTotal = o.SubTotal,
                    ShippingFee = o.ShippingFee,
                    DiscountAmount = o.DiscountAmount,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        OrderItemID = oi.OrderItemID,
                        ProductID = oi.ProductID,
                        ProductName = oi.ProductName,
                        ProductImage = oi.ProductImage,
                        SizeName = oi.SizeName,
                        ToppingName = oi.ToppingName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Note = oi.Note
                    }).ToList(),
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(ApiResponse<OrderDto>.ErrorResponse("Order not found"));
            }

            // Check if user has permission to view this order
            var userId = GetUserId();
            if (!User.IsInRole("AdminIT") && !User.IsInRole("FoodAdmin") && !User.IsInRole("Staff"))
            {
                if (order.UserID != userId)
                {
                    return Forbid();
                }
            }

            return Ok(ApiResponse<OrderDto>.SuccessResponse(order));
        }

        /// <summary>
        /// Create a new order from cart
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<OrderDto>.ErrorResponse("Invalid data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var sessionId = GetSessionId();
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserID == sessionId);

            if (cart == null || !cart.CartItems.Any())
            {
                return BadRequest(ApiResponse<OrderDto>.ErrorResponse("Cart is empty"));
            }

            var subTotal = cart.CartItems.Sum(ci => ci.TotalPrice);
            decimal discountAmount = 0;

            // Apply discount if code is provided
            if (!string.IsNullOrEmpty(dto.DiscountCode))
            {
                var discount = await _context.Discounts
                    .FirstOrDefaultAsync(d => d.Code == dto.DiscountCode
                        && d.IsActive
                        && d.StartDate <= DateTime.Now
                        && d.EndDate >= DateTime.Now
                        && (d.UsageLimit == null || d.UsageCount < d.UsageLimit)
                        && (d.MinOrderAmount == null || subTotal >= d.MinOrderAmount));

                if (discount != null)
                {
                    discountAmount = subTotal * (discount.DiscountPercent / 100);
                    if (discount.MaxDiscountAmount.HasValue && discountAmount > discount.MaxDiscountAmount.Value)
                    {
                        discountAmount = discount.MaxDiscountAmount.Value;
                    }
                    discount.UsageCount++;
                }
            }

            var totalAmount = subTotal + dto.ShippingFee - discountAmount;

            var order = new Order
            {
                UserID = GetUserId(),
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                CustomerEmail = dto.CustomerEmail,
                ShippingAddress = dto.ShippingAddress,
                Note = dto.Note,
                Status = "Pending",
                PaymentMethod = dto.PaymentMethod,
                PaymentStatus = "Pending",
                SubTotal = subTotal,
                ShippingFee = dto.ShippingFee,
                DiscountAmount = discountAmount,
                DiscountCode = dto.DiscountCode,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // Create order items from cart items
            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    ProductID = cartItem.ProductID,
                    ProductName = cartItem.ProductName,
                    ProductImage = cartItem.ProductImage,
                    SizeName = cartItem.SizeName,
                    ToppingName = cartItem.ToppingName,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    TotalPrice = cartItem.TotalPrice,
                    Note = cartItem.Note
                };
                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);

            // Clear the cart
            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            var orderDto = new OrderDto
            {
                OrderID = order.OrderID,
                UserID = order.UserID,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                CustomerEmail = order.CustomerEmail,
                ShippingAddress = order.ShippingAddress,
                Note = order.Note,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                SubTotal = order.SubTotal,
                ShippingFee = order.ShippingFee,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    OrderItemID = oi.OrderItemID,
                    ProductID = oi.ProductID,
                    ProductName = oi.ProductName,
                    ProductImage = oi.ProductImage,
                    SizeName = oi.SizeName,
                    ToppingName = oi.ToppingName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    Note = oi.Note
                }).ToList(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };

            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderID },
                ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order created successfully"));
        }

        /// <summary>
        /// Update order status (Staff/Admin only)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "AdminIT,FoodAdmin,Staff")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
            {
                return NotFound(ApiResponse<OrderDto>.ErrorResponse("Order not found"));
            }

            var validStatuses = new[] { "Pending", "Confirmed", "Preparing", "Ready", "Delivering", "Completed", "Cancelled" };
            if (!validStatuses.Contains(dto.Status))
            {
                return BadRequest(ApiResponse<OrderDto>.ErrorResponse($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}"));
            }

            order.Status = dto.Status;
            order.UpdatedAt = DateTime.Now;

            // Update payment status if order is completed
            if (dto.Status == "Completed")
            {
                order.PaymentStatus = "Paid";
            }

            await _context.SaveChangesAsync();

            var orderDto = new OrderDto
            {
                OrderID = order.OrderID,
                UserID = order.UserID,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                CustomerEmail = order.CustomerEmail,
                ShippingAddress = order.ShippingAddress,
                Note = order.Note,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                SubTotal = order.SubTotal,
                ShippingFee = order.ShippingFee,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    OrderItemID = oi.OrderItemID,
                    ProductID = oi.ProductID,
                    ProductName = oi.ProductName,
                    ProductImage = oi.ProductImage,
                    SizeName = oi.SizeName,
                    ToppingName = oi.ToppingName,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    Note = oi.Note
                }).ToList(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };

            return Ok(ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order status updated"));
        }

        /// <summary>
        /// Get orders by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetOrdersByUser(string userId)
        {
            var currentUserId = GetUserId();
            
            // Only allow users to view their own orders, or admin/staff to view any
            if (!User.IsInRole("AdminIT") && !User.IsInRole("FoodAdmin") && !User.IsInRole("Staff"))
            {
                if (currentUserId != userId)
                {
                    return Forbid();
                }
            }

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .Where(o => o.UserID == userId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new OrderDto
                {
                    OrderID = o.OrderID,
                    UserID = o.UserID,
                    CustomerName = o.CustomerName,
                    CustomerPhone = o.CustomerPhone,
                    CustomerEmail = o.CustomerEmail,
                    ShippingAddress = o.ShippingAddress,
                    Note = o.Note,
                    Status = o.Status,
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    SubTotal = o.SubTotal,
                    ShippingFee = o.ShippingFee,
                    DiscountAmount = o.DiscountAmount,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        OrderItemID = oi.OrderItemID,
                        ProductID = oi.ProductID,
                        ProductName = oi.ProductName,
                        ProductImage = oi.ProductImage,
                        SizeName = oi.SizeName,
                        ToppingName = oi.ToppingName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Note = oi.Note
                    }).ToList(),
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orders));
        }
    }
}
