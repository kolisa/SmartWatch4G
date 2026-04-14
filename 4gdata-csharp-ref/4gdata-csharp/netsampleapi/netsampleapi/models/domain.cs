using System.Text.Json.Serialization;

namespace SampleApi.Model {
    public class ResponseCode{
        [JsonPropertyName("ReturnCode")]
        public int ReturnCode { get; set; }
    }

    public class CallRecord{
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("call_number")]
        public string? CallNumber { get; set; }

        [JsonPropertyName("start_time")]
        public string? StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public string? EndTime { get; set; }
    }

    public class CallLogWithAlarm{
        [JsonPropertyName("alarm_time")]
        public string? AlarmTime { get; set; }

        [JsonPropertyName("lat")]
        public string? Lat { get; set; }

        [JsonPropertyName("lon")]
        public string? Lon { get; set; }

        [JsonPropertyName("call_logs")]
        public List<CallRecord>? CallLogs { get; set; }
    }

    public class DeviceCallLogs{
        [JsonPropertyName("deviceid")]
        public string? DeviceId { get; set; }

        [JsonPropertyName("normal_call_logs")]
        public List<CallRecord>? NormalCallLogs { get; set; }

        [JsonPropertyName("sos")]
        public List<CallLogWithAlarm>? Sos { get; set; }
    }

    public class DeviceInfo{
        [JsonPropertyName("deviceid")]
        public string DeviceId { get; set; }

        [JsonPropertyName("imsi")] 
        public string Imsi { get; set; }

        [JsonPropertyName("sn")]
        public string Sn { get; set; }

        [JsonPropertyName("mac")]
        public string Mac { get; set; }

        [JsonPropertyName("net_type")]
        public string NetType { get; set; }

        [JsonPropertyName("net_operator")]
        public string NetOperator { get; set; }

        [JsonPropertyName("wearing_status")] 
        public string WearingStatus { get; set; }

        [JsonPropertyName("model")] 
        public string Model { get; set; }

        [JsonPropertyName("version")] 
        public string Version { get; set; }

        [JsonPropertyName("sim1_iccid")] 
        public string Sim1IccId { get; set; }

        [JsonPropertyName("sim1_cellid")] 
        public string Sim1CellId { get; set; }

        [JsonPropertyName("sim1_netadhere")] 
        public string Sim1NetAdhere { get; set; }

        [JsonPropertyName("network_status")] 
        public string NetworkStatus { get; set; }

        [JsonPropertyName("band_detail")] 
        public string BandDetail { get; set; }

        [JsonPropertyName("refsignal")] 
        public string RefSignal { get; set; }

        [JsonPropertyName("band")] 
        public string Band { get; set; }

        [JsonPropertyName("communication_mode")] 
        public string CommunicationMode { get; set; }

        [JsonPropertyName("watch_event")] 
        public int WatchEvent { get; set; }
    }

    public class DeviceStatus{
        [JsonPropertyName("DeviceId")]
        public string DeviceId { get; set; }
        
        [JsonPropertyName("EventTime")]
        public string EventTime { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; }
    }

}