using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SampleApi.Utils;

namespace SampleApi.Parser {
    public class OldManProcessor{
        private readonly ILogger<OldManProcessor> logger;

        public OldManProcessor(ILogger<OldManProcessor> thelogger){
            logger = thelogger;
        }

        public void ProceedOldMan(byte[] pbData){
            var omInfo = new OM0Report();

            try{
                omInfo = OM0Report.Parser.ParseFrom(pbData);
            }
            catch (InvalidProtocolBufferException e){
                logger.LogError($"Parse oldman error: {e.Message}");
                return;
            }

            var seconds = omInfo.DateTime.DateTime_.Seconds;
            // data time
            var rtTimeStr = MyDateTimeUtils.ParsePbDateTime(seconds);

            var battery = omInfo.Battery.Level;
            var rssiUint32 = omInfo.Rssi;
            int rssi;
            if (rssiUint32 > int.MaxValue){
                rssi = -(int)(~rssiUint32 + 1);
            }
            else{
                rssi = (int)rssiUint32;
            }
            // battery and rssi
            logger.LogInformation($"----{rtTimeStr} battery:{battery}, rssi:{rssi}");

            if (omInfo.Health != null){
                var rtHealth = omInfo.Health;
                var distance = rtHealth.Distance * 0.1f;
                var calorie = rtHealth.Calorie * 0.1f;
                var step = rtHealth.Steps;
                // step/distance/calorie
                logger.LogInformation($"----{rtTimeStr} step:{step}, distance:{distance}, calorie:{calorie}");
            }

            // gnss location
            //* notice: the location upload by device is in WGS_84 coordinate system, not GCJ_02
            var trackList = omInfo.TrackData;
            if (trackList != null && trackList.Count > 0){
                foreach (var track in trackList){
                    var trackSeconds = track.Time.DateTime_.Seconds;
                    var gnssTimeStr = MyDateTimeUtils.ParsePbDateTime(trackSeconds);
                    var locateType = track.GpsType;
                    logger.LogInformation($"----gnss time:{gnssTimeStr},lon:{track.Gnss.Longitude},lat:{track.Gnss.Latitude},loc type:{locateType}");
                }
            }
        }
    }
}