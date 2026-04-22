using Google.Protobuf;
using Microsoft.Extensions.Logging;
using SmartWatch4G.Application.Utilities;

namespace SmartWatch4G.Infrastructure.Processors;

public class EcgPreprocessor
{
    private readonly ILogger<EcgPreprocessor> _logger;

    public EcgPreprocessor(ILogger<EcgPreprocessor> logger)
    {
        _logger = logger;
    }

    public void PrepareEcgData(byte[] pbData)
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

        if (hisNotify.Type == HisDataType.EcgData && hisData.Ecg != null)
        {
            string timeStr = DateTimeUtilities.FromUnixSeconds(hisData.Ecg.TimeStamp.DateTime_.Seconds);
            _logger.LogInformation("----{Time} count:{Count}", timeStr, hisData.Ecg.RawData.Count);
        }
    }
}
