using System.Reflection.Emit;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.ModelBuilder;
using ShoesShop.Business.Interfaces;
using ShoesShop.Business.Services;
using ShoesShop.Data.Context;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Interfaces;
using ShoesShop.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Controllers và tích hợp OData
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Product>("Products");
builder.Services.AddControllers()
    .AddOData(options => options
        .Select()
        .Filter()
        .OrderBy()
        .Count()
        .Expand()
        .SetMaxTop(100)
        //.AddRouteComponents("", modelBuilder.GetEdmModel())
    );
// 2. Cấu hình Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. Kết nối Cơ sở dữ liệu SQL Server (Lấy chuỗi kết nối từ appsettings.json)
builder.Services.AddDbContext<ShoeStoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 4. Đăng ký Dependency Injection (DI) cho các lớp Tầng Data & Business
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
// 5. Đăng ký AutoMapper bằng biểu thức Lambda (Sửa lỗi gạch đỏ)
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ShoesShop.Business.MappingProfile>();
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();

// 6. Cấu hình HTTP request pipeline (Middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();