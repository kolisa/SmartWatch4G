using System.Text.Json;

using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Processors;

internal sealed class AccDataHandler : IHisDataHandler
{
    private readonly IAccDataRepository _repo;
    private readonly ILogger<AccDataHandler> _logger;

    public AccDataHandler(IAccDataRepository repo, ILogger<AccDataHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public bool CanHandle(HisDataType type, HisData hisData)
        => type == HisDataType.AccelerometerData && hisData.ACCelerometerData is not null;

    public async Task HandleAsync(string deviceId, long seq, HisData hisData, CancellationToken ct)
    {
        var acc = hisData.ACCelerometerData;
        string dataTime = DateTimeUtilities.FromUnixSeconds(acc.TimeStamp.DateTime_.Seconds);
        List<int> xList = ParseBytesPairs(acc.AccX.ToByteArray());
        List<int> yList = ParseBytesPairs(acc.AccY.ToByteArray());
        List<int> zList = ParseBytesPairs(acc.AccZ.ToByteArray());

        int count = Math.Min(Math.Min(xList.Count, yList.Count), zList.Count);
        for (int i = 0; i < count; i++)
        {
            _logger.LogInformation("{Time} — ACC x:{X}, y:{Y}, z:{Z}", dataTime, xList[i], yList[i], zList[i]);
        }

        await _repo.AddAsync(new AccDataRecord
        {
            DeviceId = deviceId,
            DataTime = dataTime,
            XValuesJson = JsonSerializer.Serialize(xList),
            YValuesJson = JsonSerializer.Serialize(yList),
            ZValuesJson = JsonSerializer.Serialize(zList),
            SampleCount = count
        }, ct).ConfigureAwait(false);
    }

    private static List<int> ParseBytesPairs(byte[] bytes)
    {
        var list = new List<int>(bytes.Length / 2);
        for (int i = 1; i < bytes.Length; i += 2)
        {
            list.Add(bytes[i - 1] + (bytes[i] << 8));
        }
        return list;
    }
}
