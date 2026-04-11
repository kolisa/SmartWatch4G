using System.Net;
using System.Net.Http.Json;

using SmartWatch4G.Application.DTOs;

using Xunit;

namespace SmartWatch4G.FunctionalTests;

[Collection(ApiTestCollection.Name)]
public sealed class DeviceQueryControllerTests(TestWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    // ── GET /api/v1/devices ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAllDevices_ReturnsOkWithEmptyList()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/devices");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiListResponse<DeviceSummaryDto>? body =
            await response.Content.ReadFromJsonAsync<ApiListResponse<DeviceSummaryDto>>();

        Assert.NotNull(body);
        Assert.Equal(0, body!.ReturnCode);
        Assert.Equal(0, body.Count);
        Assert.Empty(body.Data);
    }

    [Fact]
    public async Task GetAllDevices_WithTzParam_ReturnsOk()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/devices?tz=UTC");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── GET /api/v1/devices/{deviceId} ───────────────────────────────────────

    [Fact]
    public async Task GetDevice_UnknownDeviceId_ReturnsNotFound()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/devices/UNKNOWN001");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        ApiItemResponse<DeviceDetailDto>? body =
            await response.Content.ReadFromJsonAsync<ApiItemResponse<DeviceDetailDto>>();

        Assert.NotNull(body);
        Assert.Equal(404, body!.ReturnCode);
        Assert.Null(body.Data);
    }

    [Fact]
    public async Task GetDevice_WithTzParam_ReturnsNotFound()
    {
        HttpResponseMessage response =
            await _client.GetAsync("/api/v1/devices/DEVICE001?tz=America/New_York");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── GET /api/v1/devices/{deviceId}/status ────────────────────────────────

    [Fact]
    public async Task GetDeviceStatus_ValidDate_ReturnsOkWithEmptyData()
    {
        HttpResponseMessage response =
            await _client.GetAsync("/api/v1/devices/DEVICE001/status?date=2024-01-15");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiListResponse<DeviceStatusItemDto>? body =
            await response.Content.ReadFromJsonAsync<ApiListResponse<DeviceStatusItemDto>>();

        Assert.NotNull(body);
        Assert.Equal(0, body!.ReturnCode);
        Assert.Empty(body.Data);
    }

    [Theory]
    [InlineData("/api/v1/devices/DEVICE001/status")]
    [InlineData("/api/v1/devices/DEVICE001/status?date=not-a-date")]
    [InlineData("/api/v1/devices/DEVICE001/status?date=2024-1-1")]
    public async Task GetDeviceStatus_MissingOrInvalidDate_ReturnsBadRequest(string url)
    {
        HttpResponseMessage response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ── GET /api/v1/devices/{deviceId}/status/latest ─────────────────────────

    [Fact]
    public async Task GetDeviceStatusLatest_UnknownDevice_ReturnsNotFound()
    {
        HttpResponseMessage response =
            await _client.GetAsync("/api/v1/devices/DEVICE001/status/latest");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
