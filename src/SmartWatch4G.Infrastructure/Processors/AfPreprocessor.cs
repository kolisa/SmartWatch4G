using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.Utilities;

namespace SmartWatch4G.Infrastructure.Processors;

public class AfPreprocessor
{
    private readonly ILogger<AfPreprocessor> _logger;

    public AfPreprocessor(ILogger<AfPreprocessor> logger)
    {
        _logger = logger;
    }

    public void PrepareRriData(byte[] pbData)
    {
        HisNotification hisNotify;
        try
        {
            hisNotify = HisNotification.Parser.ParseFrom(pbData);
        }
        catch (InvalidProtocolBufferException e)
        {
            _logger.LogError("Parse 80 health history error: {Message}", e.Message);
            return;
        }

        if (hisNotify.DataCase != HisNotification.DataOneofCase.HisData) return;

        var hisData = hisNotify.HisData;
        _logger.LogInformation("seq: {Seq}", hisData.Seq);

        if (hisNotify.Type == HisDataType.RriData && hisData.Rri != null)
        {
            string timeStr = DateTimeUtilities.FromUnixSeconds(hisData.Rri.TimeStamp.DateTime_.Seconds);
            var rriList = new List<long>(hisData.Rri.RawData.Count * 2);
            foreach (var raw in hisData.Rri.RawData)
            {
                long v = (long)raw;
                rriList.Add((v >> 16) & 0xffff);
                rriList.Add(v & 0xffff);
            }
            _logger.LogInformation("----{Time} count:{Count}", timeStr, rriList.Count);
        }
    }
}
