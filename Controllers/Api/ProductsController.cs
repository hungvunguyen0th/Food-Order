using Asm_GD1.Data;
using Asm_GD1.Models;
using Asm_GD1.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asm_GD1.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
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
        /// Get a product by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.ProductID == id)
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
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(ApiResponse<ProductDto>.ErrorResponse("Product not found"));
            }

            return Ok(ApiResponse<ProductDto>.SuccessResponse(product));
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<ApiResponse<List<ProductDto>>>> GetProductsByCategory(int categoryId)
        {
            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryID == categoryId);
            if (!categoryExists)
            {
                return NotFound(ApiResponse<List<ProductDto>>.ErrorResponse("Category not found"));
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.CategoryID == categoryId)
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
        /// Create a new product (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "AdminIT,FoodAdmin")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ProductDto>.ErrorResponse("Invalid data", 
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryID == dto.CategoryID);
            if (!categoryExists)
            {
                return BadRequest(ApiResponse<ProductDto>.ErrorResponse("Category not found"));
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                DiscountPrice = dto.DiscountPrice,
                DiscountPercent = dto.DiscountPercent,
                ImageUrl = dto.ImageUrl,
                CategoryID = dto.CategoryID,
                Slug = dto.Slug,
                NoteText = dto.NoteText,
                IsAvailable = dto.IsAvailable,
                PreparationTime = dto.PreparationTime,
                Calories = dto.Calories,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var result = new ProductDto
            {
                ProductID = product.ProductID,
                Name = product.Name,
                Description = product.Description,
                BasePrice = product.BasePrice,
                DiscountPrice = product.DiscountPrice,
                DiscountPercent = product.DiscountPercent,
                ImageUrl = product.ImageUrl,
                CategoryID = product.CategoryID,
                Slug = product.Slug,
                IsAvailable = product.IsAvailable,
                PreparationTime = product.PreparationTime,
                Calories = product.Calories,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.ProductID }, 
                ApiResponse<ProductDto>.SuccessResponse(result, "Product created successfully"));
        }

        /// <summary>
        /// Update a product (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "AdminIT,FoodAdmin")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<ProductDto>.ErrorResponse("Invalid data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse<ProductDto>.ErrorResponse("Product not found"));
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryID == dto.CategoryID);
            if (!categoryExists)
            {
                return BadRequest(ApiResponse<ProductDto>.ErrorResponse("Category not found"));
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.BasePrice = dto.BasePrice;
            product.DiscountPrice = dto.DiscountPrice;
            product.DiscountPercent = dto.DiscountPercent;
            product.ImageUrl = dto.ImageUrl;
            product.CategoryID = dto.CategoryID;
            product.Slug = dto.Slug;
            product.NoteText = dto.NoteText;
            product.IsAvailable = dto.IsAvailable;
            product.PreparationTime = dto.PreparationTime;
            product.Calories = dto.Calories;
            product.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var result = new ProductDto
            {
                ProductID = product.ProductID,
                Name = product.Name,
                Description = product.Description,
                BasePrice = product.BasePrice,
                DiscountPrice = product.DiscountPrice,
                DiscountPercent = product.DiscountPercent,
                ImageUrl = product.ImageUrl,
                CategoryID = product.CategoryID,
                Slug = product.Slug,
                IsAvailable = product.IsAvailable,
                PreparationTime = product.PreparationTime,
                Calories = product.Calories,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

            return Ok(ApiResponse<ProductDto>.SuccessResponse(result, "Product updated successfully"));
        }

        /// <summary>
        /// Delete a product (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "AdminIT,FoodAdmin")]
        public async Task<ActionResult<ApiResponse>> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Product not found"));
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResponse("Product deleted successfully"));
        }
    }
}
