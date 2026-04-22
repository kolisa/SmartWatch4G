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

    public Task<ServiceResult<UserResponse>> CreateAsync(CreateUserRequest request)
    {
        try
        {
            var existing = _db.GetUserProfile(request.DeviceId);
            if (existing is not null)
                return Task.FromResult(
                    ServiceResult<UserResponse>.Fail("A user with this device ID already exists.", 409));

            _db.UpsertUserProfile(request.DeviceId, request.Name, request.Surname,
                request.Email, request.Cell, request.EmpNo, request.Address);

            var created = _db.GetUserProfile(request.DeviceId);
            return Task.FromResult(created is not null
                ? ServiceResult<UserResponse>.Ok(Map(created))
                : ServiceResult<UserResponse>.Fail("Failed to retrieve user after creation.", 500));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateAsync failed for DeviceId {DeviceId}", request.DeviceId);
            return Task.FromResult(ServiceResult<UserResponse>.Fail("An unexpected error occurred.", 500));
        }
    }

    public Task<ServiceResult<UserResponse>> GetByDeviceIdAsync(string deviceId)
    {
        try
        {
            var profile = _db.GetUserProfile(deviceId);
            return Task.FromResult(profile is not null
                ? ServiceResult<UserResponse>.Ok(Map(profile))
                : ServiceResult<UserResponse>.Fail("User not found.", 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByDeviceIdAsync failed for DeviceId {DeviceId}", deviceId);
            return Task.FromResult(ServiceResult<UserResponse>.Fail("An unexpected error occurred.", 500));
        }
    }

    public Task<ServiceResult<IReadOnlyList<UserResponse>>> GetAllAsync()
    {
        try
        {
            var profiles = _db.GetAllUserProfiles();
            IReadOnlyList<UserResponse> result = profiles.Select(Map).ToList();
            return Task.FromResult(ServiceResult<IReadOnlyList<UserResponse>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllAsync failed");
            return Task.FromResult(
                ServiceResult<IReadOnlyList<UserResponse>>.Fail("An unexpected error occurred.", 500));
        }
    }

    public Task<ServiceResult<UserResponse>> UpdateAsync(string deviceId, UpdateUserRequest request)
    {
        try
        {
            var existing = _db.GetUserProfile(deviceId);
            if (existing is null)
                return Task.FromResult(ServiceResult<UserResponse>.Fail("User not found.", 404));

            _db.UpsertUserProfile(deviceId, request.Name, request.Surname,
                request.Email, request.Cell, request.EmpNo, request.Address);

            var updated = _db.GetUserProfile(deviceId);
            return Task.FromResult(updated is not null
                ? ServiceResult<UserResponse>.Ok(Map(updated))
                : ServiceResult<UserResponse>.Fail("Failed to retrieve user after update.", 500));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateAsync failed for DeviceId {DeviceId}", deviceId);
            return Task.FromResult(ServiceResult<UserResponse>.Fail("An unexpected error occurred.", 500));
        }
    }

    public Task<ServiceResult<bool>> DeleteAsync(string deviceId)
    {
        try
        {
            var existing = _db.GetUserProfile(deviceId);
            if (existing is null)
                return Task.FromResult(ServiceResult<bool>.Fail("User not found.", 404));

            _db.DeleteUserProfile(deviceId);
            return Task.FromResult(ServiceResult<bool>.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteAsync failed for DeviceId {DeviceId}", deviceId);
            return Task.FromResult(ServiceResult<bool>.Fail("An unexpected error occurred.", 500));
        }
    }

    private static UserResponse Map(UserProfile p) => new()
    {
        DeviceId  = p.DeviceId,
        Name      = p.Name,
        Surname   = p.Surname,
        Email     = p.Email,
        Cell      = p.Cell,
        EmpNo     = p.EmpNo,
        Address   = p.Address,
        UpdatedAt = p.UpdatedAt
    };
}
