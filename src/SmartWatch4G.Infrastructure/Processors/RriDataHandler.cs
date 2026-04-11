using System.Text.Json;

using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Processors;

internal sealed class RriDataHandler : IHisDataHandler
{
    private readonly IRriDataRepository _repo;
    private readonly ILogger<RriDataHandler> _logger;

    public RriDataHandler(IRriDataRepository repo, ILogger<RriDataHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public bool CanHandle(HisDataType type, HisData hisData)
        => type == HisDataType.RriData && hisData.Rri is not null;

    public async Task HandleAsync(string deviceId, long seq, HisData hisData, CancellationToken ct)
    {
        var rri = hisData.Rri;
        string dataTime = DateTimeUtilities.FromUnixSeconds(rri.TimeStamp.DateTime_.Seconds);

        var rriList = new List<long>(rri.RawData.Count * 2);
        foreach (long raw in rri.RawData)
        {
            rriList.Add((raw >> 16) & 0x0000_ffff);
            rriList.Add(raw & 0x0000_ffff);
        }

        _logger.LogDebug("{Time} — RRI samples: {Count}", dataTime, rriList.Count);

        await _repo.AddAsync(new RriDataRecord
        {
            DeviceId = deviceId,
            DataTime = dataTime,
            Seq = seq,
            SampleCount = rriList.Count,
            RriValuesJson = JsonSerializer.Serialize(rriList)
        }, ct).ConfigureAwait(false);
    }
}
