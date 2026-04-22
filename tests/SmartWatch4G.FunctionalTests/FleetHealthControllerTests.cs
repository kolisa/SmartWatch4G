using System.Net;
using System.Text.Json;
using Xunit;

namespace SmartWatch4G.FunctionalTests;

[Collection(ApiTestCollection.Name)]
public sealed class FleetHealthControllerTests(TestWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    /// <summary>
    /// The deviceinfo/upload endpoint follows the Iwown JSON protocol:
    /// it always returns HTTP 200 OK with a JSON error code in the body.
    /// ReturnCode 10002 = invalid/empty JSON body. A 400 is never returned.
    /// </summary>
    [Fact]
    public async Task DeviceInfoUpload_EmptyBody_Returns200WithErrorCode()
    {
        var response = await _client.PostAsync("/deviceinfo/upload",
            new ByteArrayContent([]));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(10002, doc.RootElement.GetProperty("ReturnCode").GetInt32());
    }
}
