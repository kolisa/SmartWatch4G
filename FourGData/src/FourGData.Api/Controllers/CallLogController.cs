using System.Text.Json;
using SmartWatch4G.Application.DTOs;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Receives JSON call-log uploads from wearable devices.
/// Route: POST /call_log/upload
/// </summary>
[Route("call_log/upload")]
[ApiController]
public sealed class CallLogController : ControllerBase
{
    private readonly ICallLogRepository _callLogRepo;
    private readonly ILogger<CallLogController> _logger;

    public CallLogController(
        ICallLogRepository callLogRepo,
        ILogger<CallLogController> logger)
    {
        _callLogRepo = callLogRepo;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> UploadCallLogAsync(CancellationToken ct)
    {
        DeviceCallLogsDto? dto;
        try
        {
            using var reader = new StreamReader(Request.Body);
            string body = await reader.ReadToEndAsync(ct).ConfigureAwait(false);
            _logger.LogInformation("UploadCallLog payload: {Body}", body);

            dto = JsonSerializer.Deserialize<DeviceCallLogsDto>(body);
            if (dto is null)
            {
                return Ok(new ResponseCodeDto { ReturnCode = 10002 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("CallLog deserialise error: {Message}", ex.Message);
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        var records = new List<CallLogRecord>();

        if (dto.NormalCallLogs is not null)
        {
            foreach (CallRecordDto call in dto.NormalCallLogs)
            {
                records.Add(new CallLogRecord
                {
                    DeviceId = dto.DeviceId ?? string.Empty,
                    CallStatus = call.Status,
                    CallNumber = call.CallNumber,
                    StartTime = call.StartTime,
                    EndTime = call.EndTime,
                    IsSosAlarm = false
                });
            }
        }

        if (dto.Sos is not null)
        {
            foreach (CallLogWithAlarmDto sos in dto.Sos)
            {
                if (sos.CallLogs is null) continue;
                foreach (CallRecordDto call in sos.CallLogs)
                {
                    records.Add(new CallLogRecord
                    {
                        DeviceId = dto.DeviceId ?? string.Empty,
                        CallStatus = call.Status,
                        CallNumber = call.CallNumber,
                        StartTime = call.StartTime,
                        EndTime = call.EndTime,
                        IsSosAlarm = true,
                        AlarmTime = sos.AlarmTime,
                        AlarmLat = sos.Lat,
                        AlarmLon = sos.Lon
                    });
                }
            }
        }

        if (records.Count > 0)
        {
            await _callLogRepo.AddRangeAsync(records, ct).ConfigureAwait(false);
        }

        return Ok(new ResponseCodeDto { ReturnCode = 0 });
    }
}
