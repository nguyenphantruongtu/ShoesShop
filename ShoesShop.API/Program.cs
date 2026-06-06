using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using ShoesShop.Business.Interfaces;
using ShoesShop.Business.Services;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories;
using ShoesShop.Data.Repositories.Interfaces;
using ShoesShop.Data.SeedData;
using System.Text;

namespace ShoesShop.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // DbContext
        builder.Services.AddDbContext<ShoeStoreDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // ── Repositories ────────────────────────────────────────────────
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IRoleRepository, RoleRepository>();
        builder.Services.AddScoped<IAddressRepository, AddressRepository>();
        // F3 – Catalog
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<IBrandRepository, BrandRepository>();
        builder.Services.AddScoped<ISizeRepository, SizeRepository>();
        builder.Services.AddScoped<IColorRepository, ColorRepository>();
        builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();

        // ── Business Services ────────────────────────────────────────────
        // F1 – Auth
        builder.Services.AddScoped<IJwtService, JwtService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        // F2 – User Profile & Address
        builder.Services.AddScoped<IUserProfileService, UserProfileService>();
        builder.Services.AddScoped<IAddressService, AddressService>();
        builder.Services.AddScoped<IAdminUserService, AdminUserService>();

        // F3 – Catalog Services
        builder.Services.AddScoped<IProductService, ProductService>();
        builder.Services.AddScoped<ICategoryService, CategoryService>();
        builder.Services.AddScoped<IBrandService, BrandService>();
        builder.Services.AddScoped<ISizeColorService, SizeColorService>();
        builder.Services.AddScoped<IProductVariantService, ProductVariantService>();

        // F10 – Order Management (Staff)
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<IOrderService, OrderService>();

        // ── AutoMapper ───────────────────────────────────────────────────
        builder.Services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<ShoesShop.Business.MappingProfile>();
        });

        // ── JWT Authentication ───────────────────────────────────────────
        var jwtKey      = builder.Configuration["JwtSettings:SecretKey"]!;
        var jwtIssuer   = builder.Configuration["JwtSettings:Issuer"]!;
        var jwtAudience = builder.Configuration["JwtSettings:Audience"]!;

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = jwtIssuer,
                    ValidAudience            = jwtAudience,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew                = TimeSpan.Zero
                };
            });

        builder.Services.AddAuthorization();

        // ── CORS ─────────────────────────────────────────────────────────
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // ── Controllers + OData ──────────────────────────────────────────
        var odataBuilder = new ODataConventionModelBuilder();
        odataBuilder.EntitySet<Product>("Products");

        builder.Services.AddControllers()
            .AddOData(options => options
                .Select()
                .Filter()
                .OrderBy()
                .Count()
                .Expand()
                .SetMaxTop(100)
            );

        // ── Swagger ──────────────────────────────────────────────────────
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ShoesShop API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name        = "Authorization",
                Type        = SecuritySchemeType.Http,
                Scheme      = "bearer",
                BearerFormat = "JWT",
                In          = ParameterLocation.Header,
                Description = "Nhập JWT token. Ví dụ: Bearer eyJhbGci..."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // ════════════════════════════════════════════════════════════════
        var app = builder.Build();

        // Seed roles
        await DbInitializer.SeedRolesAsync(app.Services);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors("AllowAll");

        app.UseAuthentication();   // phải trước UseAuthorization
        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}