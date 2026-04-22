using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SampleApi.Data;
using SampleApi.Utils;

namespace SampleApi.Parser {
    public class OldManProcessor{
        private readonly ILogger<OldManProcessor> _logger;
        private readonly DatabaseService _db;

        public OldManProcessor(ILogger<OldManProcessor> thelogger, DatabaseService db){
            _logger = thelogger;
            _db = db;
        }

        public void ProceedOldMan(byte[] pbData, string deviceId = ""){
            var omInfo = new OM0Report();

            try{
                omInfo = OM0Report.Parser.ParseFrom(pbData);
            }
            catch (InvalidProtocolBufferException e){
                _logger.LogError($"Parse oldman error: {e.Message}");
                return;
            }

            var seconds   = omInfo.DateTime.DateTime_.Seconds;
            var rtTimeStr = MyDateTimeUtils.ParsePbDateTime(seconds);

            var battery    = (int)omInfo.Battery.Level;
            var rssiUint32 = omInfo.Rssi;
            var rssi       = rssiUint32 > int.MaxValue
                ? -(int)(~rssiUint32 + 1)
                : (int)rssiUint32;

            _logger.LogInformation($"----{rtTimeStr} battery:{battery}, rssi:{rssi}");

            if (!string.IsNullOrEmpty(deviceId))
                _db.UpsertHealthSnapshot(deviceId, rtTimeStr, battery: battery, rssi: rssi);

            if (omInfo.Health != null){
                var rtHealth = omInfo.Health;
                var distance = rtHealth.Distance * 0.1f;
                var calorie  = rtHealth.Calorie  * 0.1f;
                var step     = (int)rtHealth.Steps;

                _logger.LogInformation($"----{rtTimeStr} step:{step}, distance:{distance}, calorie:{calorie}");

                if (!string.IsNullOrEmpty(deviceId))
                    _db.UpsertHealthSnapshot(deviceId, rtTimeStr,
                        steps: step, distance: distance, calorie: calorie);
            }

            // gnss location — WGS-84 coordinate system (not GCJ-02)
            var trackList = omInfo.TrackData;
            if (trackList != null && trackList.Count > 0){
                foreach (var track in trackList){
                    var trackSeconds = track.Time.DateTime_.Seconds;
                    var gnssTimeStr  = MyDateTimeUtils.ParsePbDateTime(trackSeconds);
                    var locateType   = track.GpsType.ToString();

                    _logger.LogInformation(
                        $"----gnss time:{gnssTimeStr},lon:{track.Gnss.Longitude},lat:{track.Gnss.Latitude},loc type:{locateType}");

                    if (!string.IsNullOrEmpty(deviceId))
                        _db.InsertGpsTrack(deviceId, gnssTimeStr,
                            track.Gnss.Longitude, track.Gnss.Latitude, locateType);
                }
            }
        }
    }
}
