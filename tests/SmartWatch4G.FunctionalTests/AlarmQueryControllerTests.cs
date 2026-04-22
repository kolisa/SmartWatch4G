using System.Net;
using Xunit;

namespace SmartWatch4G.FunctionalTests;

[Collection(ApiTestCollection.Name)]
public sealed class AlarmQueryControllerTests(TestWebApplicationFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    /// <summary>
    /// The alarm/upload endpoint follows the Iwown binary protocol:
    /// it always returns HTTP 200 OK with a 1-byte error code in the body.
    /// 0x02 = payload too short (minimum 23 bytes). A 400 is never returned.
    /// </summary>
    [Fact]
    public async Task AlarmUpload_EmptyBody_Returns200WithErrorByte()
    {
        var response = await _client.PostAsync("/alarm/upload",
            new ByteArrayContent([]));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(0x02, body[0]); // 0x02 = payload too short
    }
}
