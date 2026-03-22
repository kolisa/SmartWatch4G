using System.Net;
using System.Net.Http.Json;
using SmartWatch4G.Application.DTOs;
using Xunit;

namespace SmartWatch4G.FunctionalTests;

[Collection(ApiTestCollection.Name)]
public sealed class FleetHealthControllerTests(TestWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    // ── GET /api/v1/fleet/health/latest ──────────────────────────────────────

    [Fact]
    public async Task GetFleetHealthLatest_ReturnsOkWithEmptyData()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/fleet/health/latest");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiListResponse<HealthSnapshotDto>? body =
            await response.Content.ReadFromJsonAsync<ApiListResponse<HealthSnapshotDto>>();

        Assert.NotNull(body);
        Assert.Equal(0, body!.ReturnCode);
        Assert.Equal(0, body.Count);
        Assert.Empty(body.Data);
    }

    [Fact]
    public async Task GetFleetHealthLatest_WithTzParam_ReturnsOk()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/fleet/health/latest?tz=UTC");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── GET /api/v1/fleet/health/summary?date= ───────────────────────────────

    [Fact]
    public async Task GetFleetHealthSummary_ValidDate_ReturnsOkWithEmptyData()
    {
        HttpResponseMessage response =
            await _client.GetAsync("/api/v1/fleet/health/summary?date=2024-01-15");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiListResponse<HealthDailyStatsDto>? body =
            await response.Content.ReadFromJsonAsync<ApiListResponse<HealthDailyStatsDto>>();

        Assert.NotNull(body);
        Assert.Equal(0, body!.ReturnCode);
        Assert.Equal(0, body.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData("2024-1-1")]
    [InlineData("not-a-date")]
    public async Task GetFleetHealthSummary_InvalidDate_ReturnsBadRequest(string date)
    {
        HttpResponseMessage response =
            await _client.GetAsync($"/api/v1/fleet/health/summary?date={date}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
