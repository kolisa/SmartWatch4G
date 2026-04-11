using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Processors;

internal sealed class MultiLeadsEcgDataHandler : IHisDataHandler
{
    private readonly IMultiLeadsEcgRepository _repo;
    private readonly ILogger<MultiLeadsEcgDataHandler> _logger;

    public MultiLeadsEcgDataHandler(IMultiLeadsEcgRepository repo, ILogger<MultiLeadsEcgDataHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public bool CanHandle(HisDataType type, HisData hisData)
        => type == HisDataType.MultiLeadsEcgData && hisData.MultiLeadsECG is not null;

    public async Task HandleAsync(string deviceId, long seq, HisData hisData, CancellationToken ct)
    {
        var ecg = hisData.MultiLeadsECG;
        string dataTime = DateTimeUtilities.FromUnixSeconds(ecg.TimeStamp.DateTime_.Seconds);
        uint channels = ecg.NumberOfChannels;
        uint singleLen = ecg.SingleDataByteLen;
        _logger.LogDebug("{Time} — MultiLeadsECG channels: {C}, singleLen: {L}", dataTime, channels, singleLen);

        await _repo.AddAsync(new MultiLeadsEcgRecord
        {
            DeviceId = deviceId,
            DataTime = dataTime,
            Seq = seq,
            Channels = (int)channels,
            SampleByteLen = (int)singleLen,
            RawDataBase64 = Convert.ToBase64String(ecg.RawData.ToByteArray())
        }, ct).ConfigureAwait(false);
    }
}
