using System.Text.Json;
using SmartWatch4G.Application.DTOs;

namespace SmartWatch4G.Infrastructure.Services;

public static class DeviceStatusParser
{
    /// <summary>
    /// Parses the Iwown API device status response.
    /// Returns "online" when ReturnCode=0 and Data.status=1; otherwise "offline".
    /// </summary>
    public static string Parse(IwownResponse? response)
    {
        if (response is null || response.ReturnCode != 0 || response.Data is null)
            return "offline";

        try
        {
            if (response.Data is JsonElement element &&
                element.TryGetProperty("status", out var statusProp) &&
                statusProp.TryGetInt32(out var status))
            {
                return status == 1 ? "online" : "offline";
            }
        }
        catch (Exception)
        {
            // Treat any parsing error as offline
        }

        return "offline";
    }

    public static bool IsOnline(IwownResponse? response) =>
        Parse(response) == "online";
}
