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
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .Select(c => new CategoryDto
                {
                    CategoryID = c.CategoryID,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products != null ? c.Products.Count : 0,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(categories));
        }

        /// <summary>
        /// Get a category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .AsNoTracking()
                .Where(c => c.CategoryID == id)
                .Select(c => new CategoryDto
                {
                    CategoryID = c.CategoryID,
                    Name = c.Name,
                    Description = c.Description,
                    ProductCount = c.Products != null ? c.Products.Count : 0,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound(ApiResponse<CategoryDto>.ErrorResponse("Category not found"));
            }

            return Ok(ApiResponse<CategoryDto>.SuccessResponse(category));
        }

        /// <summary>
        /// Create a new category (Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "AdminIT,FoodAdmin")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<CategoryDto>.ErrorResponse("Invalid data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var result = new CategoryDto
            {
                CategoryID = category.CategoryID,
                Name = category.Name,
                Description = category.Description,
                ProductCount = 0,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return CreatedAtAction(nameof(GetCategory), new { id = category.CategoryID },
                ApiResponse<CategoryDto>.SuccessResponse(result, "Category created successfully"));
        }

        /// <summary>
        /// Update a category (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "AdminIT,FoodAdmin")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<CategoryDto>.ErrorResponse("Invalid data",
                    ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(ApiResponse<CategoryDto>.ErrorResponse("Category not found"));
            }

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            var result = new CategoryDto
            {
                CategoryID = category.CategoryID,
                Name = category.Name,
                Description = category.Description,
                ProductCount = await _context.Products.CountAsync(p => p.CategoryID == id),
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return Ok(ApiResponse<CategoryDto>.SuccessResponse(result, "Category updated successfully"));
        }

        /// <summary>
        /// Delete a category (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "AdminIT,FoodAdmin")]
        public async Task<ActionResult<ApiResponse>> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound(ApiResponse.ErrorResponse("Category not found"));
            }

            // Check if category has products
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryID == id);
            if (hasProducts)
            {
                return BadRequest(ApiResponse.ErrorResponse("Cannot delete category with existing products"));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse.SuccessResponse("Category deleted successfully"));
        }
    }
}
