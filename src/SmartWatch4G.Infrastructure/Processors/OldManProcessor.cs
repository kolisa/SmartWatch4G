using Google.Protobuf;

using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Processors;

/// <summary>
/// Parses opcode 0x0A OldMan (OM0) protobuf packets and persists GNSS track data.
/// Replaces the original <c>OldManProcessor</c> flat-class.
/// </summary>
public sealed class OldManProcessor
{
    private readonly IGnssTrackRepository _gnssRepo;
    private readonly ILogger<OldManProcessor> _logger;

    public OldManProcessor(IGnssTrackRepository gnssRepo, ILogger<OldManProcessor> logger)
    {
        _gnssRepo = gnssRepo;
        _logger = logger;
    }

    public async Task ProcessAsync(
        string deviceId,
        byte[] pbData,
        CancellationToken cancellationToken = default)
    {
        OM0Report omInfo;
        try
        {
            omInfo = OM0Report.Parser.ParseFrom(pbData);
        }
        catch (InvalidProtocolBufferException ex)
        {
            _logger.LogError("Parse OldMan (OM0) error: {Message}", ex.Message);
            return;
        }

        string rtTimeStr = DateTimeUtilities.FromUnixSeconds(omInfo.DateTime.DateTime_.Seconds);
        int battery = (int)omInfo.Battery.Level;

        uint rssiRaw = omInfo.Rssi;
        int rssi = rssiRaw > int.MaxValue
            ? -(int)(~rssiRaw + 1)
            : (int)rssiRaw;

        _logger.LogInformation("{Time} — battery: {B}%, RSSI: {R} dBm", rtTimeStr, battery, rssi);

        long? steps = null;
        float? dist = null;
        float? cal = null;

        if (omInfo.Health is not null)
        {
            steps = omInfo.Health.Steps;
            dist = omInfo.Health.Distance * 0.1f;
            cal = omInfo.Health.Calorie * 0.1f;
            _logger.LogInformation("{Time} — steps: {S}, dist: {D:F1} m, cal: {C:F1} kcal",
                rtTimeStr, steps, dist, cal);
        }

        if (omInfo.TrackData is null || omInfo.TrackData.Count == 0)
        {
            return;
        }

        // location data is in WGS-84 coordinate system
        var records = new List<GnssTrackRecord>(omInfo.TrackData.Count);
        foreach (var track in omInfo.TrackData)
        {
            string gnssTime = DateTimeUtilities.FromUnixSeconds(track.Time.DateTime_.Seconds);
            _logger.LogInformation(
                "GNSS {Time} — lon: {Lon}, lat: {Lat}, type: {Type}",
                gnssTime, track.Gnss.Longitude, track.Gnss.Latitude, track.GpsType);

            records.Add(new GnssTrackRecord
            {
                DeviceId = deviceId,
                TrackTime = gnssTime,
                Longitude = track.Gnss.Longitude,
                Latitude = track.Gnss.Latitude,
                GpsType = (int)track.GpsType,
                BatteryLevel = battery,
                Rssi = rssi,
                Steps = steps,
                DistanceMetres = dist,
                CaloriesKcal = cal
            });
        }

        await _gnssRepo.AddRangeAsync(records, cancellationToken).ConfigureAwait(false);
    }
}
