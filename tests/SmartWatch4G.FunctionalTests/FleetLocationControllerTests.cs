using System.Net;
using Xunit;

namespace SmartWatch4G.FunctionalTests;

[Collection(ApiTestCollection.Name)]
public sealed class FleetLocationControllerTests(TestWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task StatusNotify_EmptyBody_Returns400()
    {
        var response = await _client.PostAsync("/status/notify",
            new ByteArrayContent([]));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
