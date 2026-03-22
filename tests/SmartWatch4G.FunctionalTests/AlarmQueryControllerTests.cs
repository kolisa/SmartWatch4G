using System.Net;
using System.Net.Http.Json;
using SmartWatch4G.Application.DTOs;
using Xunit;

namespace SmartWatch4G.FunctionalTests;

[Collection(ApiTestCollection.Name)]
public sealed class AlarmQueryControllerTests(TestWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    // ── GET /api/v1/devices/{deviceId}/alarms/latest ─────────────────────────

    [Fact]
    public async Task GetAlarmLatest_NoData_ReturnsNotFound()
    {
        HttpResponseMessage response =
            await _client.GetAsync("/api/v1/devices/DEVICE001/alarms/latest");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        ApiItemResponse<AlarmEventDto>? body =
            await response.Content.ReadFromJsonAsync<ApiItemResponse<AlarmEventDto>>();

        Assert.NotNull(body);
        Assert.Equal(404, body!.ReturnCode);
    }

    [Fact]
    public async Task GetAlarmLatest_WithTzParam_ReturnsNotFound()
    {
        HttpResponseMessage response =
            await _client.GetAsync("/api/v1/devices/DEVICE001/alarms/latest?tz=UTC");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── GET /api/v1/devices/{deviceId}/alarms?date= ──────────────────────────

    [Fact]
    public async Task GetAlarms_ByValidDate_ReturnsOkWithEmptyData()
    {
        HttpResponseMessage response =
            await _client.GetAsync("/api/v1/devices/DEVICE001/alarms?date=2024-01-15");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiListResponse<AlarmEventDto>? body =
            await response.Content.ReadFromJsonAsync<ApiListResponse<AlarmEventDto>>();

        Assert.NotNull(body);
        Assert.Equal(0, body!.ReturnCode);
        Assert.Empty(body.Data);
    }

    [Fact]
    public async Task GetAlarms_ByValidTimeRange_ReturnsOkWithEmptyData()
    {
        string url = "/api/v1/devices/DEVICE001/alarms" +
                     "?from=2024-01-15 00:00:00&to=2024-01-15 23:59:59";

        HttpResponseMessage response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/devices/DEVICE001/alarms")]
    [InlineData("/api/v1/devices/DEVICE001/alarms?date=bad-date")]
    public async Task GetAlarms_MissingOrInvalidFilter_ReturnsBadRequest(string url)
    {
        HttpResponseMessage response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
