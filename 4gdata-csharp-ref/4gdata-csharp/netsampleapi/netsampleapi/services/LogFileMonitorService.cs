using System.Text.Json;
using System.Text.RegularExpressions;
using SampleApi.Data;

namespace SampleApi.Services
{
    public class LogFileMonitorService : BackgroundService
    {
        private readonly DatabaseService _db;
        private readonly ILogger<LogFileMonitorService> _logger;
        private readonly string _logBasePath;
        private readonly string _stateFile;

        // tracks byte offset per dated file so restarts don't re-process old lines
        private Dictionary<string, long> _offsets = new();
        private string _currentFile = "";
        private long _currentOffset = 0;

        // context carried across consecutive lines
        private string _currentDeviceId = "";

        public LogFileMonitorService(DatabaseService db, ILogger<LogFileMonitorService> logger, IConfiguration config)
        {
            _db = db;
            _logger = logger;
            _logBasePath = config["Logging:LogBasePath"] ?? "logs/myapi.log";

            var dir  = Path.GetDirectoryName(Path.GetFullPath(_logBasePath)) ?? ".";
            var stem = Path.GetFileNameWithoutExtension(_logBasePath);
            _stateFile = Path.Combine(dir, $"{stem}_monitor_state.json");
            LoadState();
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private string DailyPath()
        {
            var dir  = Path.GetDirectoryName(Path.GetFullPath(_logBasePath)) ?? ".";
            var stem = Path.GetFileNameWithoutExtension(_logBasePath);
            var ext  = Path.GetExtension(_logBasePath);
            return Path.Combine(dir, $"{stem}_{DateTime.Now:yyyy-MM-dd}{ext}");
        }

        private void LoadState()
        {
            try
            {
                if (File.Exists(_stateFile))
                    _offsets = JsonSerializer.Deserialize<Dictionary<string, long>>(
                        File.ReadAllText(_stateFile)) ?? new();
            }
            catch { _offsets = new(); }
        }

        private void SaveState()
        {
            try { File.WriteAllText(_stateFile, JsonSerializer.Serialize(_offsets)); }
            catch { }
        }

        // ── Background loop ───────────────────────────────────────────────────

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LogFileMonitorService started, base path: {Path}", _logBasePath);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var daily = DailyPath();

                    // switch to today's file if the date rolled over
                    if (_currentFile != daily)
                    {
                        _currentFile  = daily;
                        _currentOffset = _offsets.TryGetValue(daily, out var saved) ? saved : 0;
                        _logger.LogInformation("LogFileMonitor watching: {File}", daily);
                    }

                    if (File.Exists(_currentFile))
                    {
                        ReadNewLines();
                        _offsets[_currentFile] = _currentOffset;
                        SaveState();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "LogFileMonitorService loop error");
                }

                await Task.Delay(2000, stoppingToken);
            }
        }

        private void ReadNewLines()
        {
            using var fs = new FileStream(
                _currentFile, FileMode.Open, FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete);

            if (_currentOffset > fs.Length)
                _currentOffset = 0; // file was truncated/rotated

            fs.Seek(_currentOffset, SeekOrigin.Begin);

            using var reader = new StreamReader(fs);
            string? line;
            while ((line = reader.ReadLine()) != null)
                ParseLine(line);

            _currentOffset = fs.Position;
        }

        // ── Regex patterns ────────────────────────────────────────────────────

        // Strip the timestamp prefix: "4/20/2026 7:41:21 AM: Information - {msg}"
        static readonly Regex RxMsg = new(
            @"^\d+/\d+/\d+ \d+:\d+:\d+ [AP]M: \w+ - (.+)$",
            RegexOptions.Compiled);

        static readonly Regex RxDevice = new(@"^Device: (\S+)$", RegexOptions.Compiled);

        static readonly Regex RxGps = new(
            @"^----gnss time:([^,]+),lon:([^,]+),lat:([^,]+),loc type:(.+)$",
            RegexOptions.Compiled);

        static readonly Regex RxBattery = new(
            @"^----(\S+ \S+) battery:(\d+), rssi:(-?\d+)$",
            RegexOptions.Compiled);

        static readonly Regex RxStep = new(
            @"^----(\S+ \S+) step:(\d+), distance:([^,]+), calorie:(.+)$",
            RegexOptions.Compiled);

        static readonly Regex RxHr = new(
            @"^----(\S+ \S+) avg hr:(\d+), max hr:(\d+), min hr:(\d+)$",
            RegexOptions.Compiled);

        static readonly Regex RxSpo2 = new(
            @"^----(\S+ \S+) avg boxy:(\d+), max boxy:\d+, min boxy:\d+$",
            RegexOptions.Compiled);

        static readonly Regex RxBp = new(
            @"^----(\S+ \S+) sbp:(\d+), dbp:(\d+)$",
            RegexOptions.Compiled);

        static readonly Regex RxFatigue = new(
            @"^----(\S+ \S+) fatigue:(\d+)$",
            RegexOptions.Compiled);

        static readonly Regex RxLowPower = new(
            @"^----(\S+ \S+) low power alarm, battery:(\d+)$",
            RegexOptions.Compiled);

        static readonly Regex RxNotWear = new(
            @"^----(\S+ \S+) not wear alarm$",
            RegexOptions.Compiled);

        static readonly Regex RxSosAlarm = new(
            @"^---- sos alarm time (\S+ \S+)$",
            RegexOptions.Compiled);

        static readonly Regex RxCallLog = new(
            @"^UploadCallLog: (\{.+\})$",
            RegexOptions.Compiled);

        static readonly Regex RxDeviceInfo = new(
            @"^UploadDeviceInfo: (\{.+\})$",
            RegexOptions.Compiled);

        // ── Line parser ───────────────────────────────────────────────────────

        private void ParseLine(string line)
        {
            var msgMatch = RxMsg.Match(line);
            if (!msgMatch.Success) return;
            var msg = msgMatch.Groups[1].Value;

            Match m;

            // always update device context first
            if ((m = RxDevice.Match(msg)).Success)
            {
                _currentDeviceId = m.Groups[1].Value;
                return;
            }

            // UploadCallLog and UploadDeviceInfo carry device_id in the JSON
            if ((m = RxCallLog.Match(msg)).Success)    { ParseCallLog(m.Groups[1].Value);    return; }
            if ((m = RxDeviceInfo.Match(msg)).Success) { ParseDeviceInfo(m.Groups[1].Value); return; }

            // remaining patterns need a device context from a previous line
            if (string.IsNullOrEmpty(_currentDeviceId)) return;

            if ((m = RxGps.Match(msg)).Success)
            {
                _db.InsertGpsTrack(
                    _currentDeviceId,
                    m.Groups[1].Value.Trim(),
                    double.Parse(m.Groups[2].Value),
                    double.Parse(m.Groups[3].Value),
                    m.Groups[4].Value.Trim());
                return;
            }

            if ((m = RxBattery.Match(msg)).Success)
            {
                _db.UpsertHealthSnapshot(_currentDeviceId, m.Groups[1].Value,
                    battery: int.Parse(m.Groups[2].Value),
                    rssi:    int.Parse(m.Groups[3].Value));
                return;
            }

            if ((m = RxStep.Match(msg)).Success)
            {
                _db.UpsertHealthSnapshot(_currentDeviceId, m.Groups[1].Value,
                    steps:    int.Parse(m.Groups[2].Value),
                    distance: double.Parse(m.Groups[3].Value),
                    calorie:  double.Parse(m.Groups[4].Value));
                return;
            }

            if ((m = RxHr.Match(msg)).Success)
            {
                _db.UpsertHealthSnapshot(_currentDeviceId, m.Groups[1].Value,
                    avgHr: int.Parse(m.Groups[2].Value),
                    maxHr: int.Parse(m.Groups[3].Value),
                    minHr: int.Parse(m.Groups[4].Value));
                return;
            }

            if ((m = RxSpo2.Match(msg)).Success)
            {
                _db.UpsertHealthSnapshot(_currentDeviceId, m.Groups[1].Value,
                    avgSpo2: int.Parse(m.Groups[2].Value));
                return;
            }

            if ((m = RxBp.Match(msg)).Success)
            {
                _db.UpsertHealthSnapshot(_currentDeviceId, m.Groups[1].Value,
                    sbp: int.Parse(m.Groups[2].Value),
                    dbp: int.Parse(m.Groups[3].Value));
                return;
            }

            if ((m = RxFatigue.Match(msg)).Success)
            {
                _db.UpsertHealthSnapshot(_currentDeviceId, m.Groups[1].Value,
                    fatigue: int.Parse(m.Groups[2].Value));
                return;
            }

            if ((m = RxLowPower.Match(msg)).Success)
            {
                _db.InsertAlarm(_currentDeviceId, m.Groups[1].Value,
                    "low_power", $"battery:{m.Groups[2].Value}");
                return;
            }

            if ((m = RxNotWear.Match(msg)).Success)
            {
                _db.InsertAlarm(_currentDeviceId, m.Groups[1].Value, "not_wear");
                return;
            }

            if ((m = RxSosAlarm.Match(msg)).Success)
            {
                _db.InsertAlarm(_currentDeviceId, m.Groups[1].Value, "sos");
                return;
            }
        }

        // ── JSON parsers ──────────────────────────────────────────────────────

        private void ParseCallLog(string json)
        {
            try
            {
                using var doc  = JsonDocument.Parse(json);
                var root       = doc.RootElement;
                var deviceId   = root.GetProperty("deviceid").GetString() ?? _currentDeviceId;

                if (!root.TryGetProperty("sos", out var sosList)) return;

                foreach (var sos in sosList.EnumerateArray())
                {
                    var alarmTime = sos.GetProperty("alarm_time").GetString() ?? "";

                    double? lat = null, lon = null;
                    if (sos.TryGetProperty("lat", out var latEl) && double.TryParse(latEl.GetString(), out var lv)) lat = lv;
                    if (sos.TryGetProperty("lon", out var lonEl) && double.TryParse(lonEl.GetString(), out var lnv)) lon = lnv;

                    if (lat.HasValue && lon.HasValue)
                        _db.InsertGpsTrack(deviceId, alarmTime, lon.Value, lat.Value, "SOS");

                    if (!sos.TryGetProperty("call_logs", out var calls)) continue;

                    foreach (var call in calls.EnumerateArray())
                    {
                        _db.InsertSosEvent(
                            deviceId, alarmTime, lat, lon,
                            call.TryGetProperty("call_number", out var num)    ? num.GetString()    : null,
                            call.TryGetProperty("status",      out var status) ? status.GetInt32()  : null,
                            call.TryGetProperty("start_time",  out var st)     ? st.GetString()     : null,
                            call.TryGetProperty("end_time",    out var et)     ? et.GetString()     : null);
                    }
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "ParseCallLog failed"); }
        }

        private void ParseDeviceInfo(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root      = doc.RootElement;
                _db.InsertDeviceInfo(
                    root.GetProperty("deviceid").GetString() ?? _currentDeviceId,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    root.TryGetProperty("model",          out var mo) ? mo.GetString() : null,
                    root.TryGetProperty("version",        out var ve) ? ve.GetString() : null,
                    root.TryGetProperty("wearing_status", out var ws) ? ws.GetString() : null,
                    root.TryGetProperty("refsignal",      out var rs) ? rs.GetString() : null,
                    json);
            }
            catch (Exception ex) { _logger.LogError(ex, "ParseDeviceInfo failed"); }
        }
    }
}
