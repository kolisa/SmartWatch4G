using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Application.Interfaces;

public interface ICompanyService
{
    Task<ServiceResult<CompanyResponse>> CreateAsync(CreateCompanyRequest request);
    Task<ServiceResult<CompanyResponse>> GetByIdAsync(int id);
    Task<ServiceResult<IReadOnlyList<CompanyResponse>>> GetAllAsync();
    Task<ServiceResult<CompanyResponse>> UpdateAsync(int id, UpdateCompanyRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
