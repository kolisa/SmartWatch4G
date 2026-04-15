using System.Text.RegularExpressions;

namespace SampleApi.Storage
{
    /// <summary>
    /// Persists raw incoming payloads to disk so an external processor can
    /// later parse them and write the data to SQL Server via SmartWatch4G.Infrastructure.
    ///
    /// Folder layout (relative to the app's content root):
    ///
    ///   data/
    ///     {deviceId}/
    ///       {yyyy-MM-dd}/
    ///         {HH-mm-ss-fff}_{source}.bin   ← binary protobuf frames (pb, alarm)
    ///         {HH-mm-ss-fff}_{source}.json  ← JSON payloads (status, calllog, deviceinfo)
    ///
    /// A background processor (e.g. inside SmartWatch4G.Infrastructure) should:
    ///   1. Scan data/ for new files.
    ///   2. Parse each .bin through HistoryDataProcessor / AlarmProcessor etc.
    ///   3. Parse each .json through the relevant domain entity.
    ///   4. Save results to the database.
    ///   5. Move the file to processed/{deviceId}/{date}/ (or delete it).
    /// </summary>
    public class RawDataFileStore
    {
        private readonly string _baseDirectory;
        private static readonly Regex _unsafeChars = new Regex(@"[^\w\-]", RegexOptions.Compiled);

public RawDataFileStore(IWebHostEnvironment env, IConfiguration config)
    {
        string configuredPath = config["DataStorage:BasePath"] ?? "data";

        // If the configured path is absolute, use it directly.
        // If relative, resolve it against the app's content root.
        _baseDirectory = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(env.ContentRootPath, configuredPath);
        }

        /// <param name="deviceId">Device identifier extracted from the payload header or JSON body.</param>
        /// <param name="source">Label that identifies the endpoint, e.g. "pb", "alarm", "status", "calllog", "deviceinfo".</param>
        /// <param name="data">Raw bytes to persist (binary frame or UTF-8 JSON).</param>
        public async Task SaveAsync(string deviceId, string source, byte[] data)
        {
            string safeDeviceId = SanitizeDeviceId(deviceId);

            string date = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
            string timestamp = System.DateTime.UtcNow.ToString("HH-mm-ss-fff");

            string extension = source is "pb" or "alarm" ? "bin" : "json";
            string directory = Path.Combine(_baseDirectory, safeDeviceId, date);

            Directory.CreateDirectory(directory);

            string filePath = Path.Combine(directory, $"{timestamp}_{source}.{extension}");
            await File.WriteAllBytesAsync(filePath, data);
        }

        private string SanitizeDeviceId(string deviceId)
        {
            string trimmed = (deviceId ?? string.Empty).Trim().Trim('\0');
            string safe = _unsafeChars.Replace(trimmed, "_");
            return string.IsNullOrWhiteSpace(safe) ? "unknown" : safe;
        }
    }
}
