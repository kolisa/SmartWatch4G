using System.Text.RegularExpressions;

namespace KhoiWatchData.Api.Storage;

/// <summary>
/// Persists raw incoming payloads to disk.
///
/// Layout:  data/{deviceId}/{yyyy-MM-dd}/{HH-mm-ss-fff}_{source}.{bin|json}
/// </summary>
public class RawDataFileStore
{
    private readonly string _baseDirectory;
    private static readonly Regex UnsafeChars = new(@"[^\w\-]", RegexOptions.Compiled);

    public RawDataFileStore(IWebHostEnvironment env, IConfiguration config)
    {
        string configuredPath = config["DataStorage:BasePath"] ?? "data";
        _baseDirectory = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(env.ContentRootPath, configuredPath);
    }

    public async Task SaveAsync(string deviceId, string source, byte[] data)
    {
        string safeId     = Sanitize(deviceId);
        string date       = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
        string timestamp  = System.DateTime.UtcNow.ToString("HH-mm-ss-fff");
        string ext        = source is "pb" or "alarm" ? "bin" : "json";
        string directory  = Path.Combine(_baseDirectory, safeId, date);

        Directory.CreateDirectory(directory);
        await File.WriteAllBytesAsync(Path.Combine(directory, $"{timestamp}_{source}.{ext}"), data);
    }

    private static string Sanitize(string deviceId)
    {
        string trimmed = (deviceId ?? string.Empty).Trim().Trim('\0');
        string safe    = UnsafeChars.Replace(trimmed, "_");
        return string.IsNullOrWhiteSpace(safe) ? "unknown" : safe;
    }
}
