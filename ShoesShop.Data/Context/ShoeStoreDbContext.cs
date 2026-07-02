using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ShoesShop.Data.Entities;

namespace ShoesShop.Data.Context;

public partial class ShoeStoreDbContext : DbContext
{
    public ShoeStoreDbContext(DbContextOptions<ShoeStoreDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Color> Colors { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Voucher> Vouchers { get; set; }

    public virtual DbSet<Wishlist> Wishlists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__Addresse__091C2AFBAD3ECC99");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.RecipientName).HasMaxLength(100);
            entity.Property(e => e.StreetAddress).HasMaxLength(255);
            entity.Property(e => e.Ward).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Addresses_Users");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__Brands__DAD4F05EADC5B4DD");

            entity.HasIndex(e => e.BrandName, "UQ__Brands__2206CE9B7CB48D76").IsUnique();

            entity.Property(e => e.BrandName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Carts__51BCD7B76595E50C");

            entity.HasIndex(e => e.UserId, "UQ__Carts__1788CC4DA5CAF57E").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.User).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Carts_Users");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B0A42BFB5FC");

            entity.HasIndex(e => new { e.CartId, e.VariantId }, "UQ_CartItem").IsUnique();

            entity.Property(e => e.AddedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK_CartItems_Carts");

            entity.HasOne(d => d.Variant).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CartItems_Variants");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B639B8F5E");

            entity.HasIndex(e => e.Slug, "UQ__Categori__BC7B5FB63BBF9DD9").IsUnique();

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Slug).HasMaxLength(120);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_Categories_Parent");
        });

        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.ColorId).HasName("PK__Colors__8DA7674D5275A4C3");

            entity.HasIndex(e => e.ColorName, "UQ__Colors__C71A5A7B13537DDF").IsUnique();

            entity.Property(e => e.ColorName).HasMaxLength(50);
            entity.Property(e => e.HexCode).HasMaxLength(7);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E1243538CA6");

            entity.HasIndex(e => new { e.UserId, e.IsRead }, "IX_Notifications_User");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.RelatedUrl).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCF71975F96");

            entity.HasIndex(e => e.CreatedAt, "IX_Orders_CreatedAt").IsDescending();

            entity.HasIndex(e => e.PaymentStatus, "IX_Orders_PaymentStatus");

            entity.HasIndex(e => e.OrderStatus, "IX_Orders_Status");

            entity.HasIndex(e => e.UserId, "IX_Orders_User");

            entity.HasIndex(e => e.OrderCode, "UQ__Orders__999B5229CCB84167").IsUnique();

            entity.Property(e => e.CancelReason).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.District).HasMaxLength(100);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.OrderCode).HasMaxLength(30);
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(30)
                .HasDefaultValue("Pending");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Unpaid");
            entity.Property(e => e.Province).HasMaxLength(100);
            entity.Property(e => e.RecipientName).HasMaxLength(100);
            entity.Property(e => e.RecipientPhone).HasMaxLength(20);
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Ward).HasMaxLength(100);

            entity.HasOne(d => d.HandledByStaff).WithMany(p => p.OrderHandledByStaffs)
                .HasForeignKey(d => d.HandledByStaffId)
                .HasConstraintName("FK_Orders_Staff");

            entity.HasOne(d => d.User).WithMany(p => p.OrderUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Users");

            entity.HasOne(d => d.Voucher).WithMany(p => p.Orders)
                .HasForeignKey(d => d.VoucherId)
                .HasConstraintName("FK_Orders_Vouchers");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("PK__OrderIte__57ED0681EBE56CA9");

            entity.HasIndex(e => e.OrderId, "IX_OrderItems_Order");

            entity.Property(e => e.ColorName).HasMaxLength(50);
            entity.Property(e => e.LineTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.SizeValue).HasMaxLength(20);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderItems_Orders");

            entity.HasOne(d => d.Variant).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderItems_Variants");
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__OrderSta__4D7B4ABD939BAC92");

            entity.ToTable("OrderStatusHistory");

            entity.Property(e => e.ChangedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(30);

            entity.HasOne(d => d.ChangedByUser).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.ChangedByUserId)
                .HasConstraintName("FK_OrderHistory_Users");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_OrderHistory_Orders");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A383477FFE7");

            entity.HasIndex(e => e.OrderId, "IX_Payments_Order");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TransactionCode).HasMaxLength(100);

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Orders");

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Methods");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__PaymentM__DC31C1D3C14013A9");

            entity.HasIndex(e => e.MethodName, "UQ__PaymentM__218CFB17D0340FE2").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MethodName).HasMaxLength(50);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CD146F9079");

            entity.HasIndex(e => e.BrandId, "IX_Products_Brand");

            entity.HasIndex(e => e.CategoryId, "IX_Products_Category");

            entity.HasIndex(e => e.IsActive, "IX_Products_IsActive");

            entity.HasIndex(e => e.Slug, "UQ__Products__BC7B5FB6B5FE28CD").IsUnique();

            entity.Property(e => e.BasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Material).HasMaxLength(100);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.SalePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ShortDescription).HasMaxLength(500);
            entity.Property(e => e.Slug).HasMaxLength(220);

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Brands");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__ProductI__7516F70CDD5820E8");

            entity.Property(e => e.ImageUrl).HasMaxLength(500);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ProductImages_Products");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("PK__ProductV__0EA233845036D15D");

            entity.HasIndex(e => e.ProductId, "IX_Variants_Product");

            entity.HasIndex(e => e.StockQuantity, "IX_Variants_Stock");

            entity.HasIndex(e => new { e.ProductId, e.SizeId, e.ColorId }, "UQ_Variant_PSC").IsUnique();

            entity.HasIndex(e => e.Sku, "UQ__ProductV__CA1ECF0DB4285A09").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("SKU");

            entity.HasOne(d => d.Color).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ColorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Variants_Colors");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Variants_Products");

            entity.HasOne(d => d.Size).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.SizeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Variants_Sizes");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Reviews__74BC79CE4D67C1B7");

            entity.HasIndex(e => e.ProductId, "IX_Reviews_Product");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsApproved).HasDefaultValue(true);

            entity.HasOne(d => d.OrderItem).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.OrderItemId)
                .HasConstraintName("FK_Reviews_OrderItems");

            entity.HasOne(d => d.Product).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Products");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1ADB3CAFDC");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B616010C81F44").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.SizeId).HasName("PK__Sizes__83BD097AD325ABA8");

            entity.HasIndex(e => e.SizeValue, "UQ__Sizes__14F4C8E6C0FE1058").IsUnique();

            entity.Property(e => e.SizeValue).HasMaxLength(20);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CC219E864");

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => e.RoleId, "IX_Users_Role");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053413910824").IsUnique();

            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        modelBuilder.Entity<Voucher>(entity =>
        {
            entity.HasKey(e => e.VoucherId).HasName("PK__Vouchers__3AEE79219F2D6825");

            entity.HasIndex(e => e.Code, "UQ__Vouchers__A25C5AA78364F581").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.DiscountType).HasMaxLength(20);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MaxDiscountAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MinOrderAmount).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasKey(e => e.WishlistId).HasName("PK__Wishlist__233189EBBE578D14");

            entity.HasIndex(e => new { e.UserId, e.ProductId }, "UQ_Wishlist").IsUnique();

            entity.Property(e => e.AddedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Product).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Wishlists_Products");

            entity.HasOne(d => d.User).WithMany(p => p.Wishlists)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Wishlists_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
