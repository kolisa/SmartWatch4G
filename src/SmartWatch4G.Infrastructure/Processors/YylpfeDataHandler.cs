using Microsoft.Extensions.Logging;

using SmartWatch4G.Application.Utilities;
using SmartWatch4G.Domain.Entities;
using SmartWatch4G.Domain.Interfaces.Repositories;

namespace SmartWatch4G.Infrastructure.Processors;

internal sealed class YylpfeDataHandler : IHisDataHandler
{
    private readonly IYylpfeRepository _repo;
    private readonly ILogger<YylpfeDataHandler> _logger;

    public YylpfeDataHandler(IYylpfeRepository repo, ILogger<YylpfeDataHandler> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public bool CanHandle(HisDataType type, HisData hisData)
        => type == HisDataType.YylpfeData && hisData.YYLPFE is not null;

    public async Task HandleAsync(string deviceId, long seq, HisData hisData, CancellationToken ct)
    {
        var yyl = hisData.YYLPFE;
        uint dataTs = yyl.TimeStamp.DateTime_.Seconds;
        byte[] bytes = yyl.RawData.ToByteArray();

        var records = new List<YylpfeRecord>();

        for (uint i = 0; i + 11 < bytes.Length; i += 12)
        {
            uint offsetTs = 0;
            ushort areaUp = 0, areaDown = 0, rri = 0, motion = 0;
            int step = (int)i;

            for (int k = 0; k < 4; k++) offsetTs |= (uint)(bytes[step++] << (8 * k));
            for (int k = 0; k < 2; k++) areaUp |= (ushort)(bytes[step++] << (8 * k));
            for (int k = 0; k < 2; k++) areaDown |= (ushort)(bytes[step++] << (8 * k));
            for (int k = 0; k < 2; k++) rri |= (ushort)(bytes[step++] << (8 * k));
            for (int k = 0; k < 2; k++) motion |= (ushort)(bytes[step++] << (8 * k));

            uint utcTs = dataTs + offsetTs / 1000 - (8 * 3600);
            string sampleTime = DateTimeUtilities.FromUnixSeconds(utcTs);

            _logger.LogDebug("YYLPFE ts:{Ts}, areaUp:{AU}, areaDown:{AD}, rri:{R}, motion:{M}",
                utcTs, areaUp, areaDown, rri, motion);

            records.Add(new YylpfeRecord
            {
                DeviceId = deviceId,
                DataTime = sampleTime,
                Seq = seq,
                AreaUp = areaUp,
                AreaDown = areaDown,
                Rri = rri,
                Motion = motion
            });
        }

        if (records.Count > 0)
        {
            await _repo.AddRangeAsync(records, ct).ConfigureAwait(false);
        }
    }
}
