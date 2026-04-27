using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartWatch4G.Domain.Interfaces;

namespace SmartWatch4G.Jobs;

/// <summary>
/// Stateful singleton that tails dated log files and persists parsed data to the DB.
/// Triggered in real-time via <see cref="FileSystemWatcher"/> and polled every 2 s
/// by <see cref="LogFileMonitorJob"/> as a fallback so no lines are ever missed.
/// Duplicate-safe: all DB writes use IF-NOT-EXISTS / MERGE patterns and the tables
/// carry UNIQUE constraints, so re-processing the same line is always a no-op.
/// </summary>
public sealed class LogFileMonitorWorker : IDisposable
{
    private readonly IDatabaseService _db;
    private readonly ILogger<LogFileMonitorWorker> _logger;
    private readonly string _logBasePath;
    private readonly string _stateFile;

    // one active execution at a time (FSW + Quartz may both fire concurrently)
    private readonly SemaphoreSlim _lock = new(1, 1);
    private volatile bool _disposed;

    private Dictionary<string, long> _offsets = new();
    private string _currentFile   = string.Empty;
    private long   _currentOffset;

    private FileSystemWatcher? _watcher;

    public LogFileMonitorWorker(
        IDatabaseService db,
        ILogger<LogFileMonitorWorker> logger,
        IConfiguration config,
        IHostEnvironment env)
    {
        _db     = db;
        _logger = logger;

        var configured = config["Logging:LogBasePath"];
        if (string.IsNullOrWhiteSpace(configured))
        {
            _logBasePath = Path.Combine(env.ContentRootPath, "logs", "SmartWatch4gData.log");
        }
        else
        {
            _logBasePath = Path.IsPathRooted(configured)
                ? configured
                : Path.Combine(env.ContentRootPath, configured);
        }

        var dir  = Path.GetDirectoryName(Path.GetFullPath(_logBasePath)) ?? ".";
        var stem = Path.GetFileNameWithoutExtension(_logBasePath);
        _stateFile = Path.Combine(dir, $"{stem}_monitor_state.json");

        LoadState();
        SetupWatcher();
    }

    // ── FileSystemWatcher ─────────────────────────────────────────────────────

    private void SetupWatcher()
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(_logBasePath)) ?? ".";
        Directory.CreateDirectory(dir);

        _watcher = new FileSystemWatcher(dir)
        {
            Filter            = "*.log",
            NotifyFilter      = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
            IncludeSubdirectories = false,
            EnableRaisingEvents   = true
        };

        _watcher.Changed += OnFileEvent;
        _watcher.Created += OnFileEvent;

        _logger.LogInformation("LogFileMonitorWorker: watching directory {Dir} for *.log changes", dir);
    }

    private async void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        if (_disposed) return;
        if (_lock.Wait(0))
        {
            try  { await InternalExecuteAsync(); }
            catch (Exception ex) { _logger.LogError(ex, "LogFileMonitorWorker FSW error"); }
            finally { _lock.Release(); }
        }
    }

    // ── Quartz entry point (fallback polling) ─────────────────────────────────

    public async Task Execute()
    {
        if (_disposed) return;
        if (await _lock.WaitAsync(0))
        {
            try  { await InternalExecuteAsync(); }
            catch (Exception ex) { _logger.LogError(ex, "LogFileMonitorWorker poll error"); }
            finally { _lock.Release(); }
        }
    }

    // ── Core processing ───────────────────────────────────────────────────────

    private async Task InternalExecuteAsync()
    {
        var daily = DailyPath();

        if (_currentFile != daily)
        {
            _currentFile   = daily;
            _currentOffset = _offsets.TryGetValue(daily, out var saved) ? saved : 0;
            _logger.LogInformation("LogFileMonitor switched to: {File}", daily);
        }

        if (!File.Exists(_currentFile)) return;

        await ReadNewLinesAsync();
        _offsets[_currentFile] = _currentOffset;
        SaveState();
    }

    private string DailyPath()
    {
        var dir  = Path.GetDirectoryName(Path.GetFullPath(_logBasePath)) ?? ".";
        var stem = Path.GetFileNameWithoutExtension(_logBasePath);
        var ext  = Path.GetExtension(_logBasePath);
        return Path.Combine(dir, $"{stem}_{System.DateTime.Now:yyyy-MM-dd}{ext}");
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
        catch { /* best-effort */ }
    }

    private async Task ReadNewLinesAsync()
    {
        using var fs = new FileStream(
            _currentFile,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        if (_currentOffset > fs.Length)
            _currentOffset = 0;

        fs.Seek(_currentOffset, SeekOrigin.Begin);

        using var reader = new StreamReader(fs);
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
            await ParseLineAsync(line);

        _currentOffset = fs.Position;
    }

    // ── Regex patterns ────────────────────────────────────────────────────────

    static readonly Regex RxMsg = new(
        @"^\d+/\d+/\d+ \d+:\d+:\d+ [AP]M: \w+ - (.+)$",
        RegexOptions.Compiled);

    static readonly Regex RxCallLog = new(@"^UploadCallLog: (\{.+\})$",  RegexOptions.Compiled);
    static readonly Regex RxDevInfo = new(@"^UploadDeviceInfo: (\{.+\})$", RegexOptions.Compiled);

    // ── Line parser ───────────────────────────────────────────────────────────
    // GPS, health, and alarm data are written directly to the DB by OldManProcessor
    // and AlarmProcessor at request time. Parsing those same lines here would
    // re-attribute them to the wrong device when concurrent requests interleave
    // their log output. Only UploadCallLog and UploadDeviceInfo are handled here
    // because they embed the device ID inside their JSON payloads.

    private async Task ParseLineAsync(string line)
    {
        var msgMatch = RxMsg.Match(line);
        if (!msgMatch.Success) return;
        var msg = msgMatch.Groups[1].Value;

        Match m;
        if ((m = RxCallLog.Match(msg)).Success) { await ParseCallLogAsync(m.Groups[1].Value);   return; }
        if ((m = RxDevInfo.Match(msg)).Success) { await ParseDeviceInfoAsync(m.Groups[1].Value); return; }
    }

    // ── JSON parsers ──────────────────────────────────────────────────────────

    private async Task ParseCallLogAsync(string json)
    {
        try
        {
            using var doc  = JsonDocument.Parse(json);
            var root       = doc.RootElement;
            var deviceId   = root.GetProperty("deviceid").GetString() ?? string.Empty;

            if (!root.TryGetProperty("sos", out var sosList)) return;

            foreach (var sos in sosList.EnumerateArray())
            {
                var alarmTime = sos.GetProperty("alarm_time").GetString() ?? string.Empty;

                double? lat = null, lon = null;
                if (sos.TryGetProperty("lat", out var latEl) &&
                    double.TryParse(latEl.GetString(), out var lv)) lat = lv;
                if (sos.TryGetProperty("lon", out var lonEl) &&
                    double.TryParse(lonEl.GetString(), out var lnv)) lon = lnv;

                if (lat.HasValue && lon.HasValue)
                    await _db.InsertGpsTrack(deviceId, alarmTime, lon.Value, lat.Value, "SOS");

                if (!sos.TryGetProperty("call_logs", out var calls)) continue;

                foreach (var call in calls.EnumerateArray())
                    await _db.InsertSosEvent(
                        deviceId, alarmTime, lat, lon,
                        call.TryGetProperty("call_number", out var num)    ? num.GetString()    : null,
                        call.TryGetProperty("status",      out var status) ? status.GetInt32()  : null,
                        call.TryGetProperty("start_time",  out var st)     ? st.GetString()     : null,
                        call.TryGetProperty("end_time",    out var et)     ? et.GetString()     : null);
            }
        }
        catch (Exception ex) { _logger.LogError(ex, "ParseCallLog failed"); }
    }

    private async Task ParseDeviceInfoAsync(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root      = doc.RootElement;
            await _db.InsertDeviceInfo(
                root.GetProperty("deviceid").GetString() ?? string.Empty,
                System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                root.TryGetProperty("model",          out var mo) ? mo.GetString() : null,
                root.TryGetProperty("version",        out var ve) ? ve.GetString() : null,
                root.TryGetProperty("wearing_status", out var ws) ? ws.GetString() : null,
                root.TryGetProperty("refsignal",      out var rs) ? rs.GetString() : null,
                json);
        }
        catch (Exception ex) { _logger.LogError(ex, "ParseDeviceInfo failed"); }
    }

    public void Dispose()
    {
        _disposed = true;
        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }
        _lock.Dispose();
    }
}
