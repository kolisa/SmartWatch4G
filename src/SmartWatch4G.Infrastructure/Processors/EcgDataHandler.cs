using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Processors;

internal sealed class EcgDataHandler : IHisDataHandler
{
    private readonly IEcgDataRepository _repo;
    private readonly ILogger<EcgDataHandler> _logger;

    public EcgDataHandler(IEcgDataRepository repo, ILogger<EcgDataHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public bool CanHandle(HisDataType type, HisData hisData)
        => type == HisDataType.EcgData && hisData.Ecg is not null;

    public async Task HandleAsync(string deviceId, long seq, HisData hisData, CancellationToken ct)
    {
        var ecg = hisData.Ecg;
        string dataTime = DateTimeUtilities.FromUnixSeconds(ecg.TimeStamp.DateTime_.Seconds);
        string raw = Convert.ToBase64String(ecg.RawData.Select(v => (byte)v).ToArray());
        _logger.LogInformation("{Time} — ECG samples: {Count}", dataTime, ecg.RawData.Count);

        await _repo.AddAsync(new EcgDataRecord
        {
            DeviceId = deviceId,
            DataTime = dataTime,
            Seq = seq,
            SampleCount = ecg.RawData.Count,
            RawDataBase64 = raw
        }, ct).ConfigureAwait(false);
    }
}
