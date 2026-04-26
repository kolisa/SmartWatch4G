using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class UserService : IUserService
{
    private readonly IDatabaseService _db;
    private readonly ILogger<UserService> _logger;

    public UserService(IDatabaseService db, ILogger<UserService> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task<ServiceResult<UserResponse>> CreateAsync(CreateUserRequest request)
    {
        try
        {
            var existing = await _db.GetUserProfile(request.DeviceId);
            if (existing is not null)
                return ServiceResult<UserResponse>.Fail("A user with this device ID already exists.", 409);

            await _db.UpsertUserProfile(request.DeviceId, request.Name, request.Surname,
                request.Email, request.Cell, request.EmpNo, request.Address, request.CompanyId);

            var created = await _db.GetUserProfile(request.DeviceId);
            return created is not null
                ? ServiceResult<UserResponse>.Ok(Map(created))
                : ServiceResult<UserResponse>.Fail("Failed to retrieve user after creation.", 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateAsync failed for DeviceId {DeviceId}", request.DeviceId);
            return ServiceResult<UserResponse>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<UserResponse>> GetByDeviceIdAsync(string deviceId)
    {
        try
        {
            var profile = await _db.GetUserProfile(deviceId);
            return profile is not null
                ? ServiceResult<UserResponse>.Ok(Map(profile))
                : ServiceResult<UserResponse>.Fail("User not found.", 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByDeviceIdAsync failed for DeviceId {DeviceId}", deviceId);
            return ServiceResult<UserResponse>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<IReadOnlyList<UserResponse>>> GetAllAsync()
    {
        try
        {
            var profiles = await _db.GetAllUserProfiles();
            IReadOnlyList<UserResponse> result = profiles.Select(Map).ToList();
            return ServiceResult<IReadOnlyList<UserResponse>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllAsync failed");
            return ServiceResult<IReadOnlyList<UserResponse>>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<UserResponse>> UpdateAsync(string deviceId, UpdateUserRequest request)
    {
        try
        {
            var existing = await _db.GetUserProfile(deviceId);
            if (existing is null)
                return ServiceResult<UserResponse>.Fail("User not found.", 404);

            await _db.UpsertUserProfile(deviceId, request.Name, request.Surname,
                request.Email, request.Cell, request.EmpNo, request.Address);

            var updated = await _db.GetUserProfile(deviceId);
            return updated is not null
                ? ServiceResult<UserResponse>.Ok(Map(updated))
                : ServiceResult<UserResponse>.Fail("Failed to retrieve user after update.", 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateAsync failed for DeviceId {DeviceId}", deviceId);
            return ServiceResult<UserResponse>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(string deviceId)
    {
        try
        {
            var existing = await _db.GetUserProfile(deviceId);
            if (existing is null)
                return ServiceResult<bool>.Fail("User not found.", 404);

            await _db.DeleteUserProfile(deviceId);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteAsync failed for DeviceId {DeviceId}", deviceId);
            return ServiceResult<bool>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<UserResponse>> LinkToCompanyAsync(string deviceId, int? companyId)
    {
        try
        {
            var existing = await _db.GetUserProfile(deviceId);
            if (existing is null)
                return ServiceResult<UserResponse>.Fail("User not found.", 404);

            await _db.LinkUserToCompany(deviceId, companyId);
            await _db.BackfillDeviceRecords(deviceId);

            var updated = await _db.GetUserProfile(deviceId);
            return updated is not null
                ? ServiceResult<UserResponse>.Ok(Map(updated))
                : ServiceResult<UserResponse>.Fail("Failed to retrieve user after update.", 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LinkToCompanyAsync failed for DeviceId {DeviceId}", deviceId);
            return ServiceResult<UserResponse>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<IReadOnlyList<UserResponse>>> GetByCompanyIdAsync(int companyId)
    {
        try
        {
            var profiles = await _db.GetUsersByCompanyId(companyId);
            IReadOnlyList<UserResponse> result = profiles.Select(Map).ToList();
            return ServiceResult<IReadOnlyList<UserResponse>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByCompanyIdAsync failed for company {Id}", companyId);
            return ServiceResult<IReadOnlyList<UserResponse>>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<UserResponse>> ReactivateAsync(string deviceId)
    {
        try
        {
            var existing = await _db.GetUserProfile(deviceId);
            if (existing is not null && existing.IsActive)
                return ServiceResult<UserResponse>.Fail("User is already active.", 409);

            await _db.ReactivateUserProfile(deviceId);

            var reactivated = await _db.GetUserProfile(deviceId);
            return reactivated is not null
                ? ServiceResult<UserResponse>.Ok(Map(reactivated))
                : ServiceResult<UserResponse>.Fail("User not found.", 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReactivateAsync failed for DeviceId {DeviceId}", deviceId);
            return ServiceResult<UserResponse>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<int>> BackfillDeviceRecordsAsync(string deviceId)
    {
        try
        {
            var existing = await _db.GetUserProfile(deviceId);
            if (existing is null)
                return ServiceResult<int>.Fail("User not found.", 404);

            var rows = await _db.BackfillDeviceRecords(deviceId);
            return rows < 0
                ? ServiceResult<int>.Fail("Backfill encountered an error.", 500)
                : ServiceResult<int>.Ok(rows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BackfillDeviceRecordsAsync failed for DeviceId {DeviceId}", deviceId);
            return ServiceResult<int>.Fail("An unexpected error occurred.", 500);
        }
    }

    private static UserResponse Map(UserProfile p) => new()
    {
        DeviceId    = p.DeviceId,
        UserId      = p.UserId,
        Name        = p.Name,
        Surname     = p.Surname,
        Email       = p.Email,
        Cell        = p.Cell,
        EmpNo       = p.EmpNo,
        Address     = p.Address,
        CompanyId   = p.CompanyId,
        CompanyName = p.CompanyName,
        UpdatedAt   = p.UpdatedAt
    };
}
