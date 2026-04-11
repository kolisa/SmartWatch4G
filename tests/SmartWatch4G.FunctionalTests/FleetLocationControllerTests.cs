using System.Net;
using System.Net.Http.Json;

using SmartWatch4G.Application.DTOs;

using Xunit;

namespace SmartWatch4G.FunctionalTests;

[Collection(ApiTestCollection.Name)]
public sealed class FleetLocationControllerTests(TestWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    // ── GET /api/v1/fleet/location/latest ────────────────────────────────────

    [Fact]
    public async Task GetFleetLocationLatest_ReturnsOkWithEmptyData()
    {
        HttpResponseMessage response = await _client.GetAsync("/api/v1/fleet/location/latest");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiListResponse<LocationPointDto>? body =
            await response.Content.ReadFromJsonAsync<ApiListResponse<LocationPointDto>>();

        Assert.NotNull(body);
        Assert.Equal(0, body!.ReturnCode);
        Assert.Equal(0, body.Count);
        Assert.Empty(body.Data);
    }

    [Fact]
    public async Task GetFleetLocationLatest_WithTzParam_ReturnsOk()
    {
        HttpResponseMessage response =
            await _client.GetAsync("/api/v1/fleet/location/latest?tz=Europe/London");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ── GET /api/v1/fleet/location?date= ─────────────────────────────────────

    [Fact]
    public async Task GetFleetLocationByDate_ValidDate_ReturnsOkWithEmptyData()
    {
        HttpResponseMessage response =
            await _client.GetAsync("/api/v1/fleet/location?date=2024-06-01");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ApiListResponse<LocationPointDto>? body =
            await response.Content.ReadFromJsonAsync<ApiListResponse<LocationPointDto>>();

        Assert.NotNull(body);
        Assert.Equal(0, body!.ReturnCode);
        Assert.Empty(body.Data);
    }

    [Theory]
    [InlineData("/api/v1/fleet/location")]
    [InlineData("/api/v1/fleet/location?date=bad-date")]
    [InlineData("/api/v1/fleet/location?date=2024-1-1")]
    public async Task GetFleetLocationByDate_MissingOrInvalidDate_ReturnsBadRequest(string url)
    {
        HttpResponseMessage response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
