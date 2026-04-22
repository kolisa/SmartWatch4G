using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Application.Interfaces;

public interface IWorkerQueryService
{
    Task<ServiceResult<PagedResult<WorkerSummaryResponse>>> GetPagedWorkersAsync(int page, int pageSize);
    Task<ServiceResult<WorkerDetailResponse>> GetWorkerDetailAsync(string deviceId);
}
