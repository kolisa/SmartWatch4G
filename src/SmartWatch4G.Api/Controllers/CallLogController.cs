using Microsoft.AspNetCore.RateLimiting;
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
[EnableRateLimiting("device-write")]
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
        _logger.LogInformation("UploadCallLog — entry from {RemoteIp}",
            HttpContext.Connection.RemoteIpAddress);

        DeviceCallLogsDto? dto;
        try
        {
            using var reader = new StreamReader(Request.Body);
            string body = await reader.ReadToEndAsync(ct).ConfigureAwait(false);
            _logger.LogInformation("UploadCallLog payload: {Body}", body);

            dto = JsonSerializer.Deserialize<DeviceCallLogsDto>(body);
            if (dto is null)
            {
                _logger.LogWarning("UploadCallLog — deserialization returned null");
                return Ok(new ResponseCodeDto { ReturnCode = 10002 });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UploadCallLog — deserialise error");
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        if (string.IsNullOrWhiteSpace(dto.DeviceId))
        {
            _logger.LogWarning("UploadCallLog — DeviceId is null or empty, rejecting");
            return Ok(new ResponseCodeDto { ReturnCode = 10002 });
        }

        IReadOnlyList<CallLogRecord> records = BuildCallLogRecords(dto);

        if (records.Count > 0)
        {
            try
            {
                await _callLogRepo.AddRangeAsync(records, ct).ConfigureAwait(false);
                _logger.LogInformation(
                    "UploadCallLog — saved {Count} records for device {DeviceId}",
                    records.Count, dto.DeviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "UploadCallLog — DB write failed for device {DeviceId}", dto.DeviceId);
                return Ok(new ResponseCodeDto { ReturnCode = 10002 });
            }
        }
        else
        {
            _logger.LogInformation(
                "UploadCallLog — no records to save for device {DeviceId}", dto.DeviceId);
        }

        _logger.LogInformation("UploadCallLog — exit, device {DeviceId}", dto.DeviceId);
        return Ok(new ResponseCodeDto { ReturnCode = 0 });
    }

    /// <summary>
    /// Builds the flat list of <see cref="CallLogRecord"/> objects from a device payload,
    /// keeping normal call-logs and SOS alarm call-logs separate.
    /// </summary>
    private static IReadOnlyList<CallLogRecord> BuildCallLogRecords(DeviceCallLogsDto dto)
    {
        var records = new List<CallLogRecord>();

        if (dto.NormalCallLogs is not null)
        {
            foreach (CallRecordDto call in dto.NormalCallLogs)
            {
                records.Add(new CallLogRecord
                {
                    DeviceId = dto.DeviceId!,
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
                        DeviceId = dto.DeviceId!,
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

        return records;
    }
}
