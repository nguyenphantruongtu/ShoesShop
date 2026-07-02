using ShoesShop.Shared.DTOs.Voucher;

namespace ShoesShop.Business.Interfaces;

public interface IVoucherService
{
    /// <summary>UC-36: Danh sách tất cả voucher</summary>
    Task<List<VoucherResponse>> GetAllAsync();

    /// <summary>UC-36: Lấy voucher theo ID</summary>
    Task<VoucherResponse> GetByIdAsync(int voucherId);

    /// <summary>UC-36: Tạo voucher mới</summary>
    Task<VoucherResponse> CreateAsync(CreateVoucherRequest request);

    /// <summary>UC-36: Cập nhật voucher</summary>
    Task<VoucherResponse> UpdateAsync(int voucherId, UpdateVoucherRequest request);

    /// <summary>UC-36: Xóa voucher (chỉ khi chưa được dùng)</summary>
    Task DeleteAsync(int voucherId);

    /// <summary>UC-16: Kiểm tra mã voucher hợp lệ với subtotal + user hiện tại, trả về số tiền giảm (không ghi DB)</summary>
    Task<VoucherPreviewResponse> ValidateForOrderAsync(string code, decimal subTotal, int userId);
}
