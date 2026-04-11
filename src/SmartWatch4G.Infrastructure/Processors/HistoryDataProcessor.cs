using Google.Protobuf;

using Microsoft.Extensions.Logging;

namespace SmartWatch4G.Infrastructure.Processors;

/// <summary>
/// Parses opcode 0x80 history-data protobuf packets and dispatches each
/// frame to the appropriate <see cref="IHisDataHandler"/> (Strategy pattern).
/// Adding support for a new data type requires only a new handler class —
/// this dispatcher never needs to change.
/// </summary>
public sealed class HistoryDataProcessor
{
    private readonly IReadOnlyList<IHisDataHandler> _handlers;
    private readonly ILogger<HistoryDataProcessor> _logger;

    public HistoryDataProcessor(
        IEnumerable<IHisDataHandler> handlers,
        ILogger<HistoryDataProcessor> logger)
    {
        _handlers = handlers.ToList();
        _logger = logger;
    }

    public async Task ProcessAsync(
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
            _logger.LogError("Parse 0x80 history data error: {Message}", ex.Message);
            return;
        }

        if (hisNotify.DataCase == HisNotification.DataOneofCase.IndexTable)
        {
            if (hisNotify.Type == HisDataType.YylpfeData)
            {
                foreach (var index in hisNotify.IndexTable.Index)
                {
                    _logger.LogDebug(
                        "YYLPFE index — startSeq: {Start}, endSeq: {End}, time: {Time}",
                        index.StartSeq, index.EndSeq, index.Time);
                }
            }

            return;
        }

        if (hisNotify.DataCase != HisNotification.DataOneofCase.HisData)
        {
            return;
        }

        var hisData = hisNotify.HisData;
        _logger.LogDebug("HisData seq: {Seq}", hisData.Seq);

        foreach (var handler in _handlers)
        {
            if (handler.CanHandle(hisNotify.Type, hisData))
            {
                await handler.HandleAsync(deviceId, hisData.Seq, hisData, cancellationToken)
                    .ConfigureAwait(false);
                return;
            }
        }
    }
}
