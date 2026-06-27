using ShoesShop.Business.Interfaces;
using ShoesShop.Data.Entities;
using ShoesShop.Data.Repositories.Interfaces;

namespace ShoesShop.Business.Services;

public class SizeColorService : ISizeColorService
{
    private readonly ISizeRepository _sizeRepo;
    private readonly IColorRepository _colorRepo;

    public SizeColorService(ISizeRepository sizeRepo, IColorRepository colorRepo)
    {
        _sizeRepo = sizeRepo;
        _colorRepo = colorRepo;
    }

    // ---- SIZE ----
    public async Task<List<SizeResponse>> GetAllSizesAsync()
        => (await _sizeRepo.GetAllAsync()).Select(MapSize).ToList();

    public async Task<SizeResponse> CreateSizeAsync(CreateSizeRequest request)
    {
        if (await _sizeRepo.ValueExistsAsync(request.SizeValue))
            throw new InvalidOperationException($"Size '{request.SizeValue}' đã tồn tại.");

        var size = new Size { SizeValue = request.SizeValue, DisplayOrder = request.DisplayOrder };
        await _sizeRepo.AddAsync(size);
        return MapSize(size);
    }

    public async Task<SizeResponse> UpdateSizeAsync(int id, UpdateSizeRequest request)
    {
        var size = await _sizeRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy size.");

        if (await _sizeRepo.ValueExistsAsync(request.SizeValue, id))
            throw new InvalidOperationException($"Size '{request.SizeValue}' đã được dùng.");

        size.SizeValue = request.SizeValue;
        size.DisplayOrder = request.DisplayOrder;
        await _sizeRepo.UpdateAsync(size);
        return MapSize(size);
    }

    public async Task DeleteSizeAsync(int id)
    {
        var size = await _sizeRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy size.");
        await _sizeRepo.DeleteAsync(size);
    }

    // ---- COLOR ----
    public async Task<List<ColorResponse>> GetAllColorsAsync()
        => (await _colorRepo.GetAllAsync()).Select(MapColor).ToList();

    public async Task<ColorResponse> CreateColorAsync(CreateColorRequest request)
    {
        if (await _colorRepo.NameExistsAsync(request.ColorName))
            throw new InvalidOperationException($"Màu '{request.ColorName}' đã tồn tại.");

        var color = new Color { ColorName = request.ColorName, HexCode = request.HexCode };
        await _colorRepo.AddAsync(color);
        return MapColor(color);
    }

    public async Task<ColorResponse> UpdateColorAsync(int id, UpdateColorRequest request)
    {
        var color = await _colorRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy màu.");

        if (await _colorRepo.NameExistsAsync(request.ColorName, id))
            throw new InvalidOperationException($"Màu '{request.ColorName}' đã được dùng.");

        color.ColorName = request.ColorName;
        color.HexCode = request.HexCode;
        await _colorRepo.UpdateAsync(color);
        return MapColor(color);
    }

    public async Task DeleteColorAsync(int id)
    {
        var color = await _colorRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy màu.");
        await _colorRepo.DeleteAsync(color);
    }

    private static SizeResponse MapSize(Size s) => new() { SizeId = s.SizeId, SizeValue = s.SizeValue, DisplayOrder = s.DisplayOrder };
    private static ColorResponse MapColor(Color c) => new() { ColorId = c.ColorId, ColorName = c.ColorName, HexCode = c.HexCode };
}
