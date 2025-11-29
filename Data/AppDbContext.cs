using Asm_GD1.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Asm_GD1.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ===== DbSet cho các bảng dữ liệu =====
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<ProductTopping> ProductToppings { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        // Giữ tạm Account để migration dần
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Order> Orders { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // QUAN TRỌNG: Gọi base trước để Identity cấu hình các bảng
            base.OnModelCreating(modelBuilder);

            // ===== CẤU HÌNH QUAN HỆ PRODUCT =====
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Size)
                .WithMany()
                .HasForeignKey(p => p.SizeID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Topping)
                .WithMany()
                .HasForeignKey(p => p.ToppingID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== CẤU HÌNH QUAN HỆ CART - CARTITEM =====
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Size)
                .WithMany()
                .HasForeignKey(ci => ci.SizeID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Topping)
                .WithMany()
                .HasForeignKey(ci => ci.ToppingID)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // ===== CẤU HÌNH CHO PRODUCTSIZE =====
            modelBuilder.Entity<ProductSize>(entity =>
            {
                entity.HasKey(e => e.SizeID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ExtraPrice).HasColumnType("decimal(18,2)");
            });

            // ===== CẤU HÌNH CHO PRODUCTTOPPING =====
            modelBuilder.Entity<ProductTopping>(entity =>
            {
                entity.HasKey(e => e.ToppingID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ExtraPrice).HasColumnType("decimal(18,2)");
            });

            // ===== CẤU HÌNH CHO PRODUCT =====
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.BasePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountPrice).HasColumnType("decimal(18,2)");
            });

            // ===== CẤU HÌNH CHO CARTITEM =====
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            });

            // ==== CẤU HÌNH CHO DISCOUNT  =====
            modelBuilder.Entity<Discount>(entity =>
            {
                entity.HasKey(e => e.DiscountID);
                entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5,2)");
                entity.Property(e => e.MaxDiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MinOrderAmount).HasColumnType("decimal(18,2)");
            });
        }
    }
}
