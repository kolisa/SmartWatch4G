using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Application.Interfaces;
using SmartWatch4G.Domain.Common;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Services;

public sealed class CompanyService : ICompanyService
{
    private readonly IDatabaseService _db;
    private readonly ILogger<CompanyService> _logger;

    public CompanyService(IDatabaseService db, ILogger<CompanyService> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task<ServiceResult<CompanyResponse>> CreateAsync(CreateCompanyRequest request)
    {
        try
        {
            var id = await _db.CreateCompany(request.Name, request.RegistrationNumber,
                request.ContactEmail, request.ContactPhone, request.Address);

            if (id < 0)
                return ServiceResult<CompanyResponse>.Fail("Failed to create company.", 500);

            var created = await _db.GetCompany(id);
            return created is not null
                ? ServiceResult<CompanyResponse>.Ok(Map(created))
                : ServiceResult<CompanyResponse>.Fail("Failed to retrieve company after creation.", 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateAsync failed for company '{Name}'", request.Name);
            return ServiceResult<CompanyResponse>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<CompanyResponse>> GetByIdAsync(int id)
    {
        try
        {
            var company = await _db.GetCompany(id);
            return company is not null
                ? ServiceResult<CompanyResponse>.Ok(Map(company))
                : ServiceResult<CompanyResponse>.Fail("Company not found.", 404);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByIdAsync failed for id={Id}", id);
            return ServiceResult<CompanyResponse>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<IReadOnlyList<CompanyResponse>>> GetAllAsync()
    {
        try
        {
            var companies = await _db.GetAllCompanies();
            IReadOnlyList<CompanyResponse> result = companies.Select(Map).ToList();
            return ServiceResult<IReadOnlyList<CompanyResponse>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllAsync failed");
            return ServiceResult<IReadOnlyList<CompanyResponse>>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<CompanyResponse>> UpdateAsync(int id, UpdateCompanyRequest request)
    {
        try
        {
            var existing = await _db.GetCompany(id);
            if (existing is null)
                return ServiceResult<CompanyResponse>.Fail("Company not found.", 404);

            await _db.UpdateCompany(id, request.Name, request.RegistrationNumber,
                request.ContactEmail, request.ContactPhone, request.Address);

            var updated = await _db.GetCompany(id);
            return updated is not null
                ? ServiceResult<CompanyResponse>.Ok(Map(updated))
                : ServiceResult<CompanyResponse>.Fail("Failed to retrieve company after update.", 500);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateAsync failed for id={Id}", id);
            return ServiceResult<CompanyResponse>.Fail("An unexpected error occurred.", 500);
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var existing = await _db.GetCompany(id);
            if (existing is null)
                return ServiceResult<bool>.Fail("Company not found.", 404);

            await _db.DeleteCompany(id);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteAsync failed for id={Id}", id);
            return ServiceResult<bool>.Fail("An unexpected error occurred.", 500);
        }
    }

    private static CompanyResponse Map(Company c) => new()
    {
        Id                 = c.Id,
        Name               = c.Name,
        RegistrationNumber = c.RegistrationNumber,
        ContactEmail       = c.ContactEmail,
        ContactPhone       = c.ContactPhone,
        Address            = c.Address,
        CreatedAt          = c.CreatedAt,
        UpdatedAt          = c.UpdatedAt
    };

    public async Task<ServiceResult<IReadOnlyList<UserResponse>>> GetUsersAsync(int companyId)
    {
        try
        {
            var company = await _db.GetCompany(companyId);
            if (company is null)
                return ServiceResult<IReadOnlyList<UserResponse>>.Fail("Company not found.", 404);

            var profiles = await _db.GetUsersByCompanyId(companyId);
            IReadOnlyList<UserResponse> result = profiles.Select(MapUser).ToList();
            return ServiceResult<IReadOnlyList<UserResponse>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUsersAsync failed for company {Id}", companyId);
            return ServiceResult<IReadOnlyList<UserResponse>>.Fail("An unexpected error occurred.", 500);
        }
    }

    private static UserResponse MapUser(SmartWatch4G.Domain.Entities.UserProfile p) => new()
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
