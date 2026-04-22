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

    public Task<ServiceResult<CompanyResponse>> CreateAsync(CreateCompanyRequest request)
    {
        try
        {
            var id = _db.CreateCompany(request.Name, request.RegistrationNumber,
                request.ContactEmail, request.ContactPhone, request.Address);

            if (id < 0)
                return Task.FromResult(ServiceResult<CompanyResponse>.Fail("Failed to create company.", 500));

            var created = _db.GetCompany(id);
            return Task.FromResult(created is not null
                ? ServiceResult<CompanyResponse>.Ok(Map(created))
                : ServiceResult<CompanyResponse>.Fail("Failed to retrieve company after creation.", 500));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateAsync failed for company '{Name}'", request.Name);
            return Task.FromResult(ServiceResult<CompanyResponse>.Fail("An unexpected error occurred.", 500));
        }
    }

    public Task<ServiceResult<CompanyResponse>> GetByIdAsync(int id)
    {
        try
        {
            var company = _db.GetCompany(id);
            return Task.FromResult(company is not null
                ? ServiceResult<CompanyResponse>.Ok(Map(company))
                : ServiceResult<CompanyResponse>.Fail("Company not found.", 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetByIdAsync failed for id={Id}", id);
            return Task.FromResult(ServiceResult<CompanyResponse>.Fail("An unexpected error occurred.", 500));
        }
    }

    public Task<ServiceResult<IReadOnlyList<CompanyResponse>>> GetAllAsync()
    {
        try
        {
            var companies = _db.GetAllCompanies();
            IReadOnlyList<CompanyResponse> result = companies.Select(Map).ToList();
            return Task.FromResult(ServiceResult<IReadOnlyList<CompanyResponse>>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAllAsync failed");
            return Task.FromResult(
                ServiceResult<IReadOnlyList<CompanyResponse>>.Fail("An unexpected error occurred.", 500));
        }
    }

    public Task<ServiceResult<CompanyResponse>> UpdateAsync(int id, UpdateCompanyRequest request)
    {
        try
        {
            var existing = _db.GetCompany(id);
            if (existing is null)
                return Task.FromResult(ServiceResult<CompanyResponse>.Fail("Company not found.", 404));

            _db.UpdateCompany(id, request.Name, request.RegistrationNumber,
                request.ContactEmail, request.ContactPhone, request.Address);

            var updated = _db.GetCompany(id);
            return Task.FromResult(updated is not null
                ? ServiceResult<CompanyResponse>.Ok(Map(updated))
                : ServiceResult<CompanyResponse>.Fail("Failed to retrieve company after update.", 500));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateAsync failed for id={Id}", id);
            return Task.FromResult(ServiceResult<CompanyResponse>.Fail("An unexpected error occurred.", 500));
        }
    }

    public Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var existing = _db.GetCompany(id);
            if (existing is null)
                return Task.FromResult(ServiceResult<bool>.Fail("Company not found.", 404));

            _db.DeleteCompany(id);
            return Task.FromResult(ServiceResult<bool>.Ok(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteAsync failed for id={Id}", id);
            return Task.FromResult(ServiceResult<bool>.Fail("An unexpected error occurred.", 500));
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
}
