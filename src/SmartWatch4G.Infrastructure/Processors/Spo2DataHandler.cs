using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Processors;

internal sealed class Spo2DataHandler : IHisDataHandler
{
    private readonly ISpo2DataRepository _repo;
    private readonly ILogger<Spo2DataHandler> _logger;

    public Spo2DataHandler(ISpo2DataRepository repo, ILogger<Spo2DataHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public bool CanHandle(HisDataType type, HisData hisData)
        => type == HisDataType.Spo2Data && hisData.Spo2 is not null;

    public async Task HandleAsync(string deviceId, long seq, HisData hisData, CancellationToken ct)
    {
        var spo2 = hisData.Spo2;
        string dataTime = DateTimeUtilities.FromUnixSeconds(spo2.TimeStamp.DateTime_.Seconds);
        var records = new List<Spo2DataRecord>(spo2.Spo2Data.Count);

        foreach (uint raw in spo2.Spo2Data)
        {
            int spo2Val = (int)((raw >> 24) & 0xFF);
            int hr = (int)((raw >> 16) & 0xFF);
            int perfusion = (int)((raw >> 8) & 0xFF);
            int touch = (int)(raw & 0xFF);
            _logger.LogDebug("{Time} — SPO2: {S}, HR: {H}, perfusion: {P}, touch: {T}",
                dataTime, spo2Val, hr, perfusion, touch);
            records.Add(new Spo2DataRecord
            {
                DeviceId = deviceId,
                DataTime = dataTime,
                Spo2 = spo2Val,
                HeartRate = hr,
                Perfusion = perfusion,
                Touch = touch
            });
        }

        if (records.Count > 0)
        {
            await _repo.AddRangeAsync(records, ct).ConfigureAwait(false);
        }
    }
}
