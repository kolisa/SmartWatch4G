using Google.Protobuf;

using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Processors;

/// <summary>
/// Preprocesses ECG data from opcode 0x80 history packets.
/// Persists each chunk keyed by device + data_time; a downstream engine
/// combines all chunks sharing the same data_time for a single measurement.
/// Replaces the original <c>EcgPreprocessor</c> flat-class.
/// </summary>
public sealed class EcgPreprocessor
{
    private readonly IEcgDataRepository _ecgRepo;
    private readonly ILogger<EcgPreprocessor> _logger;

    public EcgPreprocessor(IEcgDataRepository ecgRepo, ILogger<EcgPreprocessor> logger)
    {
        _ecgRepo = ecgRepo;
        _logger = logger;
    }

    public async Task PrepareEcgDataAsync(
        string deviceId,
        byte[] pbData,
        CancellationToken cancellationToken = default)
    {
        HisNotification hisNotify;
        try
        {
            hisNotify = HisNotification.Parser.ParseFrom(pbData);
        }
        catch (InvalidProtocolBufferException ex)
        {
            _logger.LogError("ECG preprocessor — parse error: {Message}", ex.Message);
            return;
        }

        if (hisNotify.DataCase != HisNotification.DataOneofCase.HisData ||
            hisNotify.Type != HisDataType.EcgData ||
            hisNotify.HisData.Ecg is null)
        {
            return;
        }

        var hisEcg = hisNotify.HisData.Ecg;
        string dataTime = DateTimeUtilities.FromUnixSeconds(hisEcg.TimeStamp.DateTime_.Seconds);

        _logger.LogDebug("ECG {Time} — samples: {Count}", dataTime, hisEcg.RawData.Count);

        var rawBytes = new byte[hisEcg.RawData.Count];
        for (int i = 0; i < hisEcg.RawData.Count; i++)
        {
            rawBytes[i] = (byte)hisEcg.RawData[i];
        }

        string rawBase64 = Convert.ToBase64String(rawBytes);

        await _ecgRepo.AddAsync(new EcgDataRecord
        {
            DeviceId = deviceId,
            DataTime = dataTime,
            Seq = hisNotify.HisData.Seq,
            SampleCount = hisEcg.RawData.Count,
            RawDataBase64 = rawBase64
        }, cancellationToken).ConfigureAwait(false);
    }
}
