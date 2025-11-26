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
    [Authorize(Roles = "AdminIT,FoodAdmin,Staff")]
    public class POSController : ControllerBase
    {
        private readonly AppDbContext _context;

        public POSController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get products for POS
        /// </summary>
        [HttpGet("products")]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.IsAvailable)
                .Select(p => new ProductDto
                {
                    ProductID = p.ProductID,
                    Name = p.Name,
                    Description = p.Description,
                    BasePrice = p.BasePrice,
                    DiscountPrice = p.DiscountPrice,
                    DiscountPercent = p.DiscountPercent,
                    ImageUrl = p.ImageUrl,
                    CategoryID = p.CategoryID,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    Slug = p.Slug,
                    SoldCount = p.SoldCount,
                    Rating = p.Rating,
                    IsAvailable = p.IsAvailable,
                    PreparationTime = p.PreparationTime,
                    Calories = p.Calories,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<ProductDto>>.SuccessResponse(products));
        }

        /// <summary>
        /// Create order from POS (Staff only)
        /// </summary>
        [HttpPost("orders")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> CreatePOSOrder([FromBody] POSOrderDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<OrderDto>.ErrorResponse("Invalid data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            if (dto.Items == null || !dto.Items.Any())
            {
                return BadRequest(ApiResponse<OrderDto>.ErrorResponse("Order must have at least one item"));
            }

            var staffId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var subTotal = dto.Items.Sum(i => i.UnitPrice * i.Quantity);
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

            var totalAmount = subTotal - discountAmount;

            var order = new Order
            {
                UserID = staffId,
                CustomerName = dto.CustomerName ?? "Walk-in Customer",
                CustomerPhone = dto.CustomerPhone ?? "",
                CustomerEmail = null,
                ShippingAddress = null,
                Note = dto.Note,
                Status = "Completed",
                PaymentMethod = dto.PaymentMethod ?? "Cash",
                PaymentStatus = "Paid",
                SubTotal = subTotal,
                ShippingFee = 0,
                DiscountAmount = discountAmount,
                DiscountCode = dto.DiscountCode,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // Create order items
            foreach (var item in dto.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductID);
                if (product == null)
                {
                    return BadRequest(ApiResponse<OrderDto>.ErrorResponse($"Product with ID {item.ProductID} not found"));
                }

                var orderItem = new OrderItem
                {
                    ProductID = item.ProductID,
                    ProductName = product.Name,
                    ProductImage = product.ImageUrl,
                    SizeName = item.SizeName,
                    ToppingName = item.ToppingName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.UnitPrice * item.Quantity,
                    Note = item.Note
                };
                order.OrderItems.Add(orderItem);

                // Update sold count
                product.SoldCount += item.Quantity;
            }

            _context.Orders.Add(order);
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

            return CreatedAtAction(nameof(GetPendingOrders), 
                ApiResponse<OrderDto>.SuccessResponse(orderDto, "Order created successfully"));
        }

        /// <summary>
        /// Get pending orders for POS
        /// </summary>
        [HttpGet("orders/pending")]
        public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetPendingOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .Where(o => o.Status == "Pending" || o.Status == "Confirmed" || o.Status == "Preparing")
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

    /// <summary>
    /// DTO for POS order creation
    /// </summary>
    public class POSOrderDto
    {
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? Note { get; set; }
        public string? PaymentMethod { get; set; }
        public string? DiscountCode { get; set; }
        public List<POSOrderItemDto> Items { get; set; } = new();
    }

    public class POSOrderItemDto
    {
        public int ProductID { get; set; }
        public string? SizeName { get; set; }
        public string? ToppingName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Note { get; set; }
    }
}
