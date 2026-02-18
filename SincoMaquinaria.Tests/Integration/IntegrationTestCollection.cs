using Xunit;

namespace SincoMaquinaria.Tests.Integration;

/// <summary>
/// Defines the Integration test collection.
/// All tests in this collection will run sequentially (not in parallel)
/// to avoid database conflicts when using the shared CustomWebApplicationFactory.
/// </summary>
[CollectionDefinition("Integration", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    // This class is never instantiated.
    // It exists only to define the collection and link it to CustomWebApplicationFactory.
}
