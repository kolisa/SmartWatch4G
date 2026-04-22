namespace SmartWatch4G.Application.DTOs;

public sealed class TrackPointResponse
{
    public string GnssTime { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string? LocType { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed class OnlineUserTrackResponse
{
    public UserResponse User { get; init; } = null!;
    public TrackPointResponse LatestTrack { get; init; } = null!;
    public string DeviceStatus { get; init; } = "online";
}

public sealed class UserTrackHistoryResponse
{
    public string DeviceId { get; init; } = string.Empty;
    public string DeviceStatus { get; init; } = string.Empty;
    public IReadOnlyList<TrackPointResponse> TrackHistory { get; init; } = [];
}
