using Xunit;

namespace SmartWatch4G.FunctionalTests;

/// <summary>
/// Shared collection that causes xUnit to create a single <see cref="TestWebApplicationFactory"/>
/// instance reused across all test classes in this collection.
/// This avoids the Serilog "logger is already frozen" error that occurs when a second
/// WebApplicationFactory attempts to re-run the host startup code.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ApiTestCollection : ICollectionFixture<TestWebApplicationFactory>
{
    public const string Name = "API";
}
