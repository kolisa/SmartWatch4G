using System.Net;
using Xunit;

namespace SmartWatch4G.FunctionalTests;

[Collection(ApiTestCollection.Name)]
public sealed class FleetHealthControllerTests(TestWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task DeviceInfoUpload_EmptyBody_Returns400()
    {
        var response = await _client.PostAsync("/deviceinfo/upload",
            new ByteArrayContent([]));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
