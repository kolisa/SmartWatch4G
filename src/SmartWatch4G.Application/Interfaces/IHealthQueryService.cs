using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Common;
using System;

namespace SmartWatch4G.Application.Interfaces;

public interface IHealthQueryService
{
    /// <summary>Paged health records for all devices in a company with date filters.</summary>
    Task<ServiceResult<HealthPagedResult>> GetByCompanyAsync(int companyId, HealthQueryParams q);

    /// <summary>Per-device health aggregates (averages, totals) for a company.</summary>
    Task<ServiceResult<IReadOnlyList<HealthSummaryResponse>>> GetSummaryByCompanyAsync(int companyId, DateTime? from, DateTime? to);

    /// <summary>Paged health records for a single device with date filters.</summary>
    Task<ServiceResult<HealthPagedResult>> GetByDeviceAsync(string deviceId, HealthQueryParams q);

    /// <summary>Latest single health snapshot for a device.</summary>
    Task<ServiceResult<HealthRecordResponse>> GetLatestByDeviceAsync(string deviceId);
}
