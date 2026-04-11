using System.Text.Json;

using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Processors;

internal sealed class PpgDataHandler : IHisDataHandler
{
    private readonly IPpgDataRepository _repo;
    private readonly ILogger<PpgDataHandler> _logger;

    public PpgDataHandler(IPpgDataRepository repo, ILogger<PpgDataHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public bool CanHandle(HisDataType type, HisData hisData)
        => type == HisDataType.PpgData && hisData.Ppg is not null;

    public async Task HandleAsync(string deviceId, long seq, HisData hisData, CancellationToken ct)
    {
        var ppg = hisData.Ppg;
        string dataTime = DateTimeUtilities.FromUnixSeconds(ppg.TimeStamp.DateTime_.Seconds);

        var pairs = new List<int[]>(ppg.RawData.Count);
        foreach (int raw in ppg.RawData)
        {
            short first = (short)Math.Abs((raw >> 16) & 0x0000_FFFF);
            short second = (short)Math.Abs(raw & 0x0000_FFFF);
            pairs.Add([first, second]);
        }

        _logger.LogDebug("{Time} — PPG samples: {Count}", dataTime, pairs.Count);

        await _repo.AddAsync(new PpgDataRecord
        {
            DeviceId = deviceId,
            DataTime = dataTime,
            Seq = seq,
            SampleCount = pairs.Count,
            RawDataJson = JsonSerializer.Serialize(pairs)
        }, ct).ConfigureAwait(false);
    }
}
