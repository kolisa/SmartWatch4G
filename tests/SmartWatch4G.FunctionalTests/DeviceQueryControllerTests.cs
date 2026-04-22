using System.Net;
using Xunit;

namespace SmartWatch4G.FunctionalTests;

[Collection(ApiTestCollection.Name)]
public sealed class DeviceQueryControllerTests(TestWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PbUpload_EmptyBody_Returns400()
    {
        var response = await _client.PostAsync("/pb/upload",
            new ByteArrayContent([]));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
