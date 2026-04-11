namespace SmartWatch4G.Infrastructure.Processors;

/// <summary>
/// Strategy interface for handling a single protobuf history-data packet type.
/// Implementations are discovered and dispatched by <see cref="HistoryDataProcessor"/>.
/// </summary>
public interface IHisDataHandler
{
    /// <summary>
    /// Returns <c>true</c> when this handler is responsible for the given packet.
    /// Both the <paramref name="type"/> discriminator and the populated sub-field
    /// on <paramref name="hisData"/> are tested so that V1/V2 ThirdParty variants
    /// can be routed independently.
    /// </summary>
    bool CanHandle(HisDataType type, HisData hisData);

    /// <summary>Parses and persists the relevant fields from <paramref name="hisData"/>.</summary>
    Task HandleAsync(string deviceId, long seq, HisData hisData, CancellationToken ct);
}
