using Xunit;

namespace SmartWatch4G.FunctionalTests;

[CollectionDefinition(Name)]
public sealed class ApiTestCollection : ICollectionFixture<TestWebApplicationFactory>
{
    public const string Name = "API";
}
