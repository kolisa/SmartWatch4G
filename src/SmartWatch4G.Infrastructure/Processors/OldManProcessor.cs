using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Infrastructure.Processors;

public class OldManProcessor
{
    private readonly ILogger<OldManProcessor> _logger;
    private readonly IDatabaseService _db;

    public OldManProcessor(ILogger<OldManProcessor> logger, IDatabaseService db)
    {
        _logger = logger;
        _db = db;
    }

    public void ProceedOldMan(byte[] pbData, string deviceId = "")
    {
        OM0Report omInfo;
        try
        {
            omInfo = OM0Report.Parser.ParseFrom(pbData);
        }
        catch (InvalidProtocolBufferException e)
        {
            _logger.LogError("Parse oldman error: {Message}", e.Message);
            return;
        }

        var seconds   = omInfo.DateTime.DateTime_.Seconds;
        var rtTimeStr = DateTimeUtilities.FromUnixSeconds(seconds);

        var battery    = (int)omInfo.Battery.Level;
        var rssiUint32 = omInfo.Rssi;
        var rssi       = rssiUint32 > int.MaxValue
            ? -(int)(~rssiUint32 + 1)
            : (int)rssiUint32;

        _logger.LogInformation("----{Time} battery:{Battery}, rssi:{Rssi}", rtTimeStr, battery, rssi);

        if (!string.IsNullOrEmpty(deviceId))
            _db.UpsertHealthSnapshot(deviceId, rtTimeStr, battery: battery, rssi: rssi);

        if (omInfo.Health != null)
        {
            var rtHealth = omInfo.Health;
            var distance = rtHealth.Distance * 0.1f;
            var calorie  = rtHealth.Calorie  * 0.1f;
            var step     = (int)rtHealth.Steps;

            _logger.LogInformation("----{Time} step:{Step}, distance:{Distance}, calorie:{Calorie}",
                rtTimeStr, step, distance, calorie);

            if (!string.IsNullOrEmpty(deviceId))
                _db.UpsertHealthSnapshot(deviceId, rtTimeStr,
                    steps: step, distance: distance, calorie: calorie);
        }

        // gnss location — WGS-84 coordinate system (not GCJ-02)
        var trackList = omInfo.TrackData;
        if (trackList != null && trackList.Count > 0)
        {
            foreach (var track in trackList)
            {
                var trackSeconds = track.Time.DateTime_.Seconds;
                var gnssTimeStr  = DateTimeUtilities.FromUnixSeconds(trackSeconds);
                var locateType   = track.GpsType.ToString();

                _logger.LogInformation(
                    "----gnss time:{GnssTime},lon:{Lon},lat:{Lat},loc type:{LocType}",
                    gnssTimeStr, track.Gnss.Longitude, track.Gnss.Latitude, locateType);

                if (!string.IsNullOrEmpty(deviceId))
                    _db.InsertGpsTrack(deviceId, gnssTimeStr,
                        track.Gnss.Longitude, track.Gnss.Latitude, locateType);
            }
        }
    }
}
