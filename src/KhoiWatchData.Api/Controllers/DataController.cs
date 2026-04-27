using System.Text;
using KhoiWatchData.Api.Storage;
using Microsoft.AspNetCore.Mvc;
using SmartWatch4G.Infrastructure.Processors;

namespace KhoiWatchData.Api.Controllers;

[Route("pb/upload")]
[ApiController]
public class DataController : ControllerBase
{
    private readonly ILogger<DataController> _logger;
    private readonly HistoryDataProcessor _historyDataParser;
    private readonly OldManProcessor _oldmanParser;
    private readonly AfPreprocessor _afPreprocessor;
    private readonly EcgPreprocessor _ecgPreprocessor;
    private readonly SleepPreprocessor _sleepPreprocessor;
    private readonly RawDataFileStore _rawDataStore;

    public DataController(
        ILogger<DataController> logger,
        HistoryDataProcessor historyDataParser,
        OldManProcessor oldmanParser,
        AfPreprocessor afPreprocessor,
        EcgPreprocessor ecgPreprocessor,
        SleepPreprocessor sleepPreprocessor,
        RawDataFileStore rawDataStore)
    {
        _logger           = logger;
        _historyDataParser = historyDataParser;
        _oldmanParser      = oldmanParser;
        _afPreprocessor    = afPreprocessor;
        _ecgPreprocessor   = ecgPreprocessor;
        _sleepPreprocessor = sleepPreprocessor;
        _rawDataStore      = rawDataStore;
    }

    [HttpPost]
    public async Task<ActionResult> UploadPbData()
    {
        byte[] payload;
        try
        {
            using var memoryStream = new MemoryStream();
            await Request.Body.CopyToAsync(memoryStream);
            payload = memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError("Read post data failed: {Message}", ex.Message);
            return File(new byte[] { 0x01 }, "text/plain");
        }

        if (payload.Length < 23)
        {
            _logger.LogWarning("[pb/upload] Payload too short: received {Bytes} bytes, minimum is 23", payload.Length);
            return File(new byte[] { 0x02 }, "text/plain");
        }

        string hexString = BitConverter.ToString(payload).Replace("-", "");
        _logger.LogInformation("receive data: {HexString}", hexString);

        var deviceBytes = new byte[15];
        var prefixBytes = new byte[2];
        var lenBytes    = new byte[2];
        var crcBytes    = new byte[2];
        var optBytes    = new byte[2];

        Array.Copy(payload, 0, deviceBytes, 0, 15);
        string device = Encoding.UTF8.GetString(deviceBytes).Trim('\0', ' ', '\r', '\n');
        _logger.LogInformation("Device: {Device}", device);

        try
        {
            await _rawDataStore.SaveAsync(device, "pb", payload);
            _logger.LogInformation("[pb/upload] Raw payload saved for device {Device} ({Bytes} bytes)", device.Trim(), payload.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[pb/upload] Failed to save raw payload for device {Device}", device.Trim());
        }

        int startPos = 15;
        while (true)
        {
            if (payload.Length < startPos + 8)
            {
                _logger.LogWarning("Data length below {Length}", startPos + 8);
                return File(new byte[] { 0x02 }, "text/plain");
            }

            Array.Copy(payload, startPos, prefixBytes, 0, 2);
            if (prefixBytes[0] != 0x44 || prefixBytes[1] != 0x54)
            {
                _logger.LogWarning("Invalid data header at {Position}", startPos);
                return File(new byte[] { 0x03 }, "text/plain");
            }

            Array.Copy(payload, startPos + 2, lenBytes, 0, 2);
            int length = lenBytes[1] * 0x100 + lenBytes[0];

            Array.Copy(payload, startPos + 4, crcBytes, 0, 2);
            Array.Copy(payload, startPos + 6, optBytes, 0, 2);

            if (payload.Length < startPos + 8 + length)
            {
                _logger.LogWarning("Data length below {Length}", startPos + 8 + length);
                return File(new byte[] { 0x02 }, "text/plain");
            }

            var pbPayload = new byte[length];
            Array.Copy(payload, startPos + 8, pbPayload, 0, length);

            ushort opt = BitConverter.ToUInt16(optBytes, 0);

            switch (opt)
            {
                case 0x0A:
                    await _oldmanParser.ProceedOldMan(pbPayload, device);
                    break;
                case 0x80:
                    _historyDataParser.ProceedHistoryData(pbPayload);
                    _sleepPreprocessor.PrepareSleepData(pbPayload);
                    _ecgPreprocessor.PrepareEcgData(pbPayload);
                    _afPreprocessor.PrepareRriData(pbPayload);
                    break;
            }

            startPos += 8 + length;
            if (payload.Length == startPos) break;
        }

        return File(new byte[] { 0x00 }, "text/plain");
    }
}
