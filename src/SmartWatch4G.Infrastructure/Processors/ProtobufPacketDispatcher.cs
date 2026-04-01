using Microsoft.Extensions.Logging;

using SmartWatch4G.Domain.Interfaces.Services;

namespace SmartWatch4G.Infrastructure.Processors;

/// <summary>
/// Routes decoded binary packets to the correct processor based on opcode.
/// Implements <see cref="IProtobufPacketHandler"/> so the API layer depends only
/// on the domain abstraction, not the concrete processors.
/// </summary>
public sealed class ProtobufPacketDispatcher : IProtobufPacketHandler
{
    // Opcodes as defined in the original protocol
    private const ushort OpcodeOldMan = 0x0A;
    private const ushort OpcodeHistoryData = 0x80;
    private const ushort OpcodeAlarmV2 = 0x12;

    private readonly HistoryDataProcessor _historyProcessor;
    private readonly OldManProcessor _oldManProcessor;
    private readonly AlarmProcessor _alarmProcessor;
    private readonly AfPreprocessor _afPreprocessor;
    private readonly EcgPreprocessor _ecgPreprocessor;
    private readonly SleepPreprocessor _sleepPreprocessor;
    private readonly ILogger<ProtobufPacketDispatcher> _logger;

    public ProtobufPacketDispatcher(
        HistoryDataProcessor historyProcessor,
        OldManProcessor oldManProcessor,
        AlarmProcessor alarmProcessor,
        AfPreprocessor afPreprocessor,
        EcgPreprocessor ecgPreprocessor,
        SleepPreprocessor sleepPreprocessor,
        ILogger<ProtobufPacketDispatcher> logger)
    {
        _historyProcessor = historyProcessor;
        _oldManProcessor = oldManProcessor;
        _alarmProcessor = alarmProcessor;
        _afPreprocessor = afPreprocessor;
        _ecgPreprocessor = ecgPreprocessor;
        _sleepPreprocessor = sleepPreprocessor;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task HandleAsync(
        string deviceId,
        ushort opcode,
        byte[] payload,
        CancellationToken cancellationToken = default)
    {
        switch (opcode)
        {
            case OpcodeOldMan:
                await _oldManProcessor
                    .ProcessAsync(deviceId, payload, cancellationToken)
                    .ConfigureAwait(false);
                break;

            case OpcodeHistoryData:
                // Run all 0x80 processors; each filters internally for its data type
                await _historyProcessor
                    .ProcessAsync(deviceId, payload, cancellationToken)
                    .ConfigureAwait(false);
                await _sleepPreprocessor
                    .PrepareSleepDataAsync(deviceId, payload, cancellationToken)
                    .ConfigureAwait(false);
                await _ecgPreprocessor
                    .PrepareEcgDataAsync(deviceId, payload, cancellationToken)
                    .ConfigureAwait(false);
                await _afPreprocessor
                    .PrepareRriDataAsync(deviceId, payload, cancellationToken)
                    .ConfigureAwait(false);
                break;

            case OpcodeAlarmV2:
                await _alarmProcessor
                    .ProcessAsync(deviceId, payload, cancellationToken)
                    .ConfigureAwait(false);
                break;

            default:
                _logger.LogWarning("Unknown opcode 0x{Opcode:X4} from device {DeviceId} — ignored.", opcode, deviceId);
                break;
        }
    }
}
