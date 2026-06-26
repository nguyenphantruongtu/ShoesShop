using ShoesShop.Shared.DTOs;

namespace ShoesShop.Business.Interfaces;

public interface IExportService
{
    IReadOnlyList<ExportTypeDto> GetExportTypes();
    Task<(byte[] Content, string FileName)> ExportCsvAsync(string type, DateTime? from = null, DateTime? to = null, int top = 20);
}
