using Marten;
using Xunit;

namespace SincoMaquinaria.Tests;

/// <summary>
/// Base class for integration tests following Dometrain patterns:
/// - Fresh session per test
/// - No shared state cleanup needed (schema is isolated)
/// </summary>
public abstract class IntegrationContext : IAsyncLifetime, IClassFixture<IntegrationFixture>
{
    protected readonly IntegrationFixture _fixture;
    protected IDocumentSession CurrentSession { get; private set; } = null!;

    protected IntegrationContext(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync()
    {
        // Create a fresh session for each test
        CurrentSession = _fixture.Store.LightweightSession();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (CurrentSession != null)
        {
            await CurrentSession.DisposeAsync();
        }
    }
    
    /// <summary>
    /// Helper to save changes and get a fresh session for verification.
    /// Following Dometrain pattern: new session after SaveChanges.
    /// </summary>
    protected async Task SaveChangesAsync()
    {
        await CurrentSession.SaveChangesAsync();
    }
    
    /// <summary>
    /// Creates a new session for verification after saving changes.
    /// Use this when you need to verify data was persisted correctly.
    /// </summary>
    protected IDocumentSession CreateVerificationSession()
    {
        return _fixture.Store.LightweightSession();
    }
}
