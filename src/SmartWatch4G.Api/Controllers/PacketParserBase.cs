using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace SmartWatch4G.Api.Controllers;

/// <summary>
/// Shared binary frame-parsing logic for the binary upload endpoints
/// (<c>/pb/upload</c> and <c>/alarm/upload</c>).
///
/// Protocol layout:
/// <code>
/// [0..14]  15 bytes  — device ID (UTF-8, null-padded)
/// repeat {
///   [+0..+1]   2 bytes  — prefix 0x44 0x54
///   [+2..+3]   2 bytes  — payload length (little-endian)
///   [+4..+5]   2 bytes  — CRC  (not verified here)
///   [+6..+7]   2 bytes  — opcode (little-endian)
///   [+8..]     N bytes  — protobuf payload
/// }
/// </code>
/// </summary>
public abstract class PacketParserBase : ControllerBase
{
    private const int DeviceIdLength = 15;
    private const int FrameHeaderLength = 8;
    private const byte PrefixByte0 = 0x44;
    private const byte PrefixByte1 = 0x54;

    /// <summary>
    /// Reads the raw HTTP body, validates the binary frame and calls
    /// <see cref="OnPacketAsync"/> for every valid opcode/payload pair.
    /// Returns the appropriate 1-byte response file on success or error.
    /// </summary>
    protected async Task<IActionResult> ParseAndDispatchAsync(CancellationToken ct)
    {
        byte[] body;
        try
        {
            using var ms = new MemoryStream();
            await Request.Body.CopyToAsync(ms, ct).ConfigureAwait(false);
            body = ms.ToArray();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to read request body");
            return BinaryResponse(0x01);
        }

        // Minimum: 15-byte device ID + 8-byte frame header = 23 bytes
        if (body.Length < DeviceIdLength + FrameHeaderLength)
        {
            Logger.LogWarning("Payload too short: {Length} bytes", body.Length);
            return BinaryResponse(0x02);
        }

        byte[] deviceIdBytes = new byte[DeviceIdLength];
        Array.Copy(body, 0, deviceIdBytes, 0, DeviceIdLength);
        string deviceId = Encoding.UTF8.GetString(deviceIdBytes).TrimEnd('\0');
        Logger.LogInformation("Device: {DeviceId}", deviceId);

        int pos = DeviceIdLength;
        while (pos < body.Length)
        {
            if (body.Length < pos + FrameHeaderLength)
            {
                Logger.LogWarning("Truncated frame header at position {Pos}", pos);
                return BinaryResponse(0x02);
            }

            if (body[pos] != PrefixByte0 || body[pos + 1] != PrefixByte1)
            {
                Logger.LogWarning("Invalid frame prefix at position {Pos}", pos);
                return BinaryResponse(0x03);
            }

            int payloadLen = body[pos + 3] * 0x100 + body[pos + 2]; // little-endian
            ushort opcode = BitConverter.ToUInt16(body, pos + 6);

            if (body.Length < pos + FrameHeaderLength + payloadLen)
            {
                Logger.LogWarning("Payload length {Len} exceeds remaining bytes at position {Pos}", payloadLen, pos);
                return BinaryResponse(0x02);
            }

            byte[] payload = new byte[payloadLen];
            Array.Copy(body, pos + FrameHeaderLength, payload, 0, payloadLen);

            Logger.LogInformation(
                "Processing opcode 0x{Opcode:X4}, payload {PayloadLen} bytes for device {DeviceId}",
                opcode, payloadLen, deviceId);

            try
            {
                await OnPacketAsync(deviceId, opcode, payload, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "OnPacketAsync failed — device {DeviceId}, opcode 0x{Opcode:X4}",
                    deviceId, opcode);
                return BinaryResponse(0x01);
            }

            pos += FrameHeaderLength + payloadLen;
        }

        return BinaryResponse(0x00);
    }

    /// <summary>Called for each successfully parsed packet.</summary>
    protected abstract Task OnPacketAsync(
        string deviceId,
        ushort opcode,
        byte[] payload,
        CancellationToken ct);

    /// <summary>Returns a raw 1-byte binary response as per the device protocol.</summary>
    protected IActionResult BinaryResponse(byte code)
        => File(new byte[] { code }, "text/plain");

    /// <summary>Logger provided by the concrete controller.</summary>
    protected abstract ILogger Logger { get; }
}
