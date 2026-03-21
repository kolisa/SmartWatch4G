namespace SmartWatch4G.Domain.Interfaces.Services;

/// <summary>
/// Handles a decoded protobuf payload for a given opcode.
/// Implementations live in the Infrastructure layer.
/// </summary>
public interface IProtobufPacketHandler
{
    /// <summary>
    /// Asynchronously processes a single decoded protobuf payload.
    /// </summary>
    /// <param name="deviceId">The 15-byte device identifier from the packet header.</param>
    /// <param name="opcode">Two-byte opcode from the packet header.</param>
    /// <param name="payload">Raw protobuf bytes.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task HandleAsync(string deviceId, ushort opcode, byte[] payload, CancellationToken cancellationToken = default);
}
