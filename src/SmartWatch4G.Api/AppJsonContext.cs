using System.Text.Json.Serialization;

using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Api;

/// <summary>
/// System.Text.Json source-generation context for all API DTO types.
/// Generates metadata at compile time, eliminating runtime reflection and
/// removing serialization overhead that matters at 100 000+ device scale.
/// Wire this into <c>AddControllers().AddJsonOptions()</c> in Program.cs.
/// </summary>
[JsonSerializable(typeof(ResponseCodeDto))]
[JsonSerializable(typeof(ApiListResponse<DeviceSummaryDto>))]
[JsonSerializable(typeof(ApiItemResponse<DeviceDetailDto>))]
[JsonSerializable(typeof(ApiListResponse<DeviceStatusItemDto>))]
[JsonSerializable(typeof(ApiItemResponse<DeviceStatusItemDto>))]
[JsonSerializable(typeof(ApiListResponse<HealthSnapshotDto>))]
[JsonSerializable(typeof(ApiItemResponse<HealthSnapshotDto>))]
[JsonSerializable(typeof(ApiItemResponse<HealthDailyStatsDto>))]
[JsonSerializable(typeof(ApiListResponse<Spo2ReadingDto>))]
[JsonSerializable(typeof(ApiListResponse<EcgRecordDto>))]
[JsonSerializable(typeof(ApiListResponse<RriReadingDto>))]
[JsonSerializable(typeof(ApiListResponse<LocationPointDto>))]
[JsonSerializable(typeof(ApiItemResponse<LocationPointDto>))]
[JsonSerializable(typeof(ApiListResponse<AccelerometerReadingDto>))]
[JsonSerializable(typeof(ApiListResponse<AlarmEventDto>))]
[JsonSerializable(typeof(ApiItemResponse<AlarmEventDto>))]
[JsonSerializable(typeof(ApiListResponse<CallLogItemDto>))]
[JsonSerializable(typeof(ApiListResponse<SleepTrendItemDto>))]
[JsonSerializable(typeof(SleepResponseDto))]
[JsonSerializable(typeof(ApiItemResponse<SleepResultDto>))]
[JsonSerializable(typeof(ApiItemResponse<RhythmAnalysisDto>))]
[JsonSerializable(typeof(ApiItemResponse<Spo2AnalysisDto>))]
[JsonSerializable(typeof(ApiItemResponse<ParkinsonAnalysisDto>))]
[JsonSerializable(typeof(ApiListResponse<ThirdPartyDataDto>))]
[JsonSerializable(typeof(ApiListResponse<MultiLeadsEcgDto>))]
[JsonSerializable(typeof(ApiListResponse<PpgReadingDto>))]
[JsonSerializable(typeof(ApiListResponse<YylpfeReadingDto>))]
[JsonSerializable(typeof(CommandResultDto))]
[JsonSerializable(typeof(DeviceOnlineStatusDto))]
// Device write payloads (inbound deserialization)
[JsonSerializable(typeof(DeviceInfoDto))]
[JsonSerializable(typeof(DeviceStatusDto))]
[JsonSerializable(typeof(DeviceCallLogsDto))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.Unspecified, // keep PascalCase
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false)]
public partial class AppJsonContext : JsonSerializerContext;
