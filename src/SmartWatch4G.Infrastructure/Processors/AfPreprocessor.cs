using System.Text.Json;

using Google.Protobuf;

using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Processors;

/// <summary>
/// Preprocesses RRI data from opcode 0x80 history packets for AF calculation.
/// Persists the decoded RRI sequences so a downstream engine can later combine
/// any desired time range and compute the AF result.
/// Replaces the original <c>AfPreprocessor</c> flat-class.
/// </summary>
public sealed class AfPreprocessor
{
    private readonly IRriDataRepository _rriRepo;
    private readonly ILogger<AfPreprocessor> _logger;

    public AfPreprocessor(IRriDataRepository rriRepo, ILogger<AfPreprocessor> logger)
    {
        _rriRepo = rriRepo;
        _logger = logger;
    }

    public async Task PrepareRriDataAsync(
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
            _logger.LogError("AF preprocessor — parse error: {Message}", ex.Message);
            return;
        }

        if (hisNotify.DataCase != HisNotification.DataOneofCase.HisData ||
            hisNotify.Type != HisDataType.RriData ||
            hisNotify.HisData.Rri is null)
        {
            return;
        }

        var hisRri = hisNotify.HisData.Rri;
        string dataTime = DateTimeUtilities.FromUnixSeconds(hisRri.TimeStamp.DateTime_.Seconds);

        // Unpack two 16-bit values packed into each uint32
        var rriList = new List<long>(hisRri.RawData.Count * 2);
        foreach (long raw in hisRri.RawData)
        {
            rriList.Add((raw >> 16) & 0x0000_ffff);
            rriList.Add(raw & 0x0000_ffff);
        }

        _logger.LogDebug("AF RRI {Time} — count: {Count}", dataTime, rriList.Count);

        await _rriRepo.AddAsync(new RriDataRecord
        {
            DeviceId = deviceId,
            DataTime = dataTime,
            Seq = hisNotify.HisData.Seq,
            SampleCount = rriList.Count,
            RriValuesJson = JsonSerializer.Serialize(rriList)
        }, cancellationToken).ConfigureAwait(false);
    }
}
