using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Processors;

internal sealed class HealthDataHandler : IHisDataHandler
{
    private readonly IHealthDataRepository _repo;
    private readonly ILogger<HealthDataHandler> _logger;

    public HealthDataHandler(IHealthDataRepository repo, ILogger<HealthDataHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public bool CanHandle(HisDataType type, HisData hisData)
        => type == HisDataType.HealthData && hisData.Health is not null;

    public async Task HandleAsync(string deviceId, long seq, HisData hisData, CancellationToken ct)
    {
        var h = hisData.Health;
        string dataTime = DateTimeUtilities.FromUnixSeconds(h.TimeStamp.DateTime_.Seconds);
        var record = new HealthDataRecord { DeviceId = deviceId, DataTime = dataTime, Seq = seq };

        if (h.PedoData is not null)
        {
            record.Steps = h.PedoData.Step;
            record.DistanceMetres = h.PedoData.Distance * 0.1f;
            record.CaloriesKcal = h.PedoData.Calorie * 0.1f;
            record.ActivityType = h.PedoData.Type;
            record.ActivityState = h.PedoData.State & 15u;
            _logger.LogDebug("{Time} — steps: {S}, dist: {D:F1} m, cal: {C:F1} kcal",
                dataTime, record.Steps, record.DistanceMetres, record.CaloriesKcal);
        }

        if (h.HrData is not null)
        {
            record.AvgHeartRate = h.HrData.AvgBpm;
            record.MaxHeartRate = h.HrData.MaxBpm;
            record.MinHeartRate = h.HrData.MinBpm;
            _logger.LogDebug("{Time} — HR avg: {A}, max: {X}, min: {N}",
                dataTime, record.AvgHeartRate, record.MaxHeartRate, record.MinHeartRate);
        }

        if (h.BxoyData is not null)
        {
            record.AvgSpo2 = h.BxoyData.AgvOxy;
            record.MaxSpo2 = h.BxoyData.MaxOxy;
            record.MinSpo2 = h.BxoyData.MinOxy;
        }

        if (h.BpData is not null)
        {
            record.Sbp = h.BpData.Sbp;
            record.Dbp = h.BpData.Dbp;
            _logger.LogDebug("{Time} — BP: {Sbp}/{Dbp}", dataTime, record.Sbp, record.Dbp);
        }

        if (h.HrvData is not null)
        {
            record.HrvSdnn = h.HrvData.SDNN / 10.0;
            record.HrvRmssd = h.HrvData.RMSSD / 10.0;
            record.HrvPnn50 = h.HrvData.PNN50 / 10.0;
            record.HrvMean = h.HrvData.MEAN / 10.0;
            int fatigue = (int)h.HrvData.Fatigue;
            if (fatigue <= 0) fatigue = (int)(Math.Log(h.HrvData.RMSSD) * 20);
            record.Fatigue = fatigue;
            _logger.LogDebug("{Time} — fatigue: {F}", dataTime, fatigue);
        }

        if (h.TemperatureData is not null)
        {
            record.TemperatureIsValid = (int)h.TemperatureData.Type;
            record.AxillaryTemp = (h.TemperatureData.EstiArm & 0x0000_ffff) / 100.0f;
            record.EstimatedTemp = ((h.TemperatureData.EstiArm >> 16) & 0x0000_ffff) / 100.0f;
            record.ShellTemp = (h.TemperatureData.EviBody & 0x0000_ffff) / 100.0f;
            record.EnvTemp = ((h.TemperatureData.EviBody >> 16) & 0x0000_ffff) / 100.0f;
        }

        if (h.SleepData is not null)
        {
            _logger.LogDebug("{Time} — sleep entries: {Count}, charge: {C}, shutdown: {S}",
                dataTime, h.SleepData.SleepData.Count, h.SleepData.Charge, h.SleepData.ShutDown);
        }

        if (h.BiozData is not null)
        {
            record.BiozR = (int?)h.BiozData.R;
            record.BiozX = (int?)h.BiozData.X;
            record.BodyFat = h.BiozData.Fat;
            record.Bmi = h.BiozData.Bmi;
        }

        if (h.BloodSugarData is not null) record.BloodSugar = h.BloodSugarData.BloodSugar;
        if (h.BloodPotassiumData is not null) record.BloodPotassium = h.BloodPotassiumData.BloodPotassium;

        if (h.BpBpmData is not null)
        {
            record.BpBpm = h.BpBpmData.Bpm;
            _logger.LogDebug("{Time} — BP BPM: {B}", dataTime, record.BpBpm);
        }

        if (h.HumitureData is not null)
        {
            record.MatressHumidity = h.HumitureData.Humidity;
            record.MatressTemperature = h.HumitureData.Temperature;
            _logger.LogDebug("{Time} — mattress humidity: {H}%, temp: {T}°C",
                dataTime, record.MatressHumidity, record.MatressTemperature);
        }

        await _repo.AddAsync(record, ct).ConfigureAwait(false);
    }
}
