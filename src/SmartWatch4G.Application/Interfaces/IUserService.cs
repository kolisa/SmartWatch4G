using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Application.Interfaces;

public interface IUserService
{
    Task<ServiceResult<UserResponse>> CreateAsync(CreateUserRequest request);
    Task<ServiceResult<UserResponse>> GetByDeviceIdAsync(string deviceId);
    Task<ServiceResult<IReadOnlyList<UserResponse>>> GetAllAsync();
    Task<ServiceResult<UserResponse>> UpdateAsync(string deviceId, UpdateUserRequest request);
    Task<ServiceResult<bool>> DeleteAsync(string deviceId);
    Task<ServiceResult<UserResponse>> LinkToCompanyAsync(string deviceId, int? companyId);

    Task<ServiceResult<IReadOnlyList<UserResponse>>> GetByCompanyIdAsync(int companyId);

    Task<ServiceResult<UserResponse>> ReactivateAsync(string deviceId);

    /// <summary>Backfills user_id and company_id into every historical data row for the device.</summary>
    Task<ServiceResult<int>> BackfillDeviceRecordsAsync(string deviceId);
}
