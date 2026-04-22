using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Common;

namespace SmartWatch4G.Application.Interfaces;

public interface ITrackingQueryService
{
    Task<ServiceResult<IReadOnlyList<OnlineUserTrackResponse>>> GetOnlineUsersWithTrackingAsync();
    Task<ServiceResult<UserTrackHistoryResponse>> GetTrackHistoryAsync(string deviceId, System.DateTime? from, System.DateTime? to);
}
