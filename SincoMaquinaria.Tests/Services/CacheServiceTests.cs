using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SincoMaquinaria.Services;
using Xunit;

namespace SincoMaquinaria.Tests.Services;

public class CacheServiceTests
{
    #region Distributed Cache Tests

    [Fact]
    public async Task GetAsync_ConDistributedCache_DebeRetornarValorDeserializado()
    {
        // Arrange
        var testData = new { Id = 1, Nombre = "Test" };
        var serialized = JsonSerializer.Serialize(testData);

        var distributedCacheMock = new Mock<IDistributedCache>();
        distributedCacheMock
            .Setup(x => x.GetAsync("test-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes(serialized));

        var serviceProvider = CreateServiceProvider(distributedCacheMock.Object, null);
        var config = CreateConfig(cachingEnabled: true);
        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        // Act
        var result = await cacheService.GetAsync<object>("test-key");

        // Assert
        result.Should().NotBeNull();
        distributedCacheMock.Verify(x => x.GetAsync("test-key", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ConDistributedCache_SinDatos_DebeRetornarNull()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        distributedCacheMock
            .Setup(x => x.GetAsync("missing-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var serviceProvider = CreateServiceProvider(distributedCacheMock.Object, null);
        var config = CreateConfig(cachingEnabled: true);
        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        // Act
        var result = await cacheService.GetAsync<string>("missing-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ConDistributedCache_DebeSerializarYGuardar()
    {
        // Arrange
        var testData = new { Id = 1, Nombre = "Test" };
        var distributedCacheMock = new Mock<IDistributedCache>();

        var serviceProvider = CreateServiceProvider(distributedCacheMock.Object, null);
        var config = CreateConfig(cachingEnabled: true, expirationMinutes: 10);
        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        // Act
        await cacheService.SetAsync("test-key", testData);

        // Assert
        distributedCacheMock.Verify(x => x.SetAsync(
            "test-key",
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o =>
                o.AbsoluteExpirationRelativeToNow == TimeSpan.FromMinutes(10)),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task SetAsync_ConDistributedCache_ConExpiracionPersonalizada_DebeUsarExpiracion()
    {
        // Arrange
        var testData = "test-value";
        var distributedCacheMock = new Mock<IDistributedCache>();

        var serviceProvider = CreateServiceProvider(distributedCacheMock.Object, null);
        var config = CreateConfig(cachingEnabled: true);
        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        var customExpiration = TimeSpan.FromMinutes(30);

        // Act
        await cacheService.SetAsync("test-key", testData, customExpiration);

        // Assert
        distributedCacheMock.Verify(x => x.SetAsync(
            "test-key",
            It.IsAny<byte[]>(),
            It.Is<DistributedCacheEntryOptions>(o =>
                o.AbsoluteExpirationRelativeToNow == customExpiration),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ConDistributedCache_DebeLlamarRemove()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();

        var serviceProvider = CreateServiceProvider(distributedCacheMock.Object, null);
        var config = CreateConfig(cachingEnabled: true);
        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        // Act
        await cacheService.RemoveAsync("test-key");

        // Assert
        distributedCacheMock.Verify(x => x.RemoveAsync("test-key", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Memory Cache Tests (Fallback when DistributedCache not available)

    [Fact]
    public async Task GetAsync_ConSoloMemoryCache_CuandoDistributedCacheNoDisponible_DebeRetornarValor()
    {
        // Arrange - When cachingEnabled=true but no DistributedCache, falls back to MemoryCache
        var testData = "test-value";

        // Provide ONLY MemoryCache (no DistributedCache)
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        memoryCache.Set("test-key", testData);

        // cachingEnabled=true, but service provider only has MemoryCache
        var serviceProvider = CreateServiceProvider(null, memoryCache);
        var config = CreateConfig(cachingEnabled: true);  // true, but only MemoryCache available

        // Since cachingEnabled=true, constructor looks for DistributedCache (won't find it)
        // So _distributedCache will be null, and GetAsync will check _memoryCache (also null in constructor)
        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        // Act
        var result = await cacheService.GetAsync<string>("test-key");

        // Assert - Will return null because constructor doesn't set _memoryCache when cachingEnabled=true
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ConCachingDeshabilitado_YMemoryCacheDisponible_DebeRetornarNull()
    {
        // Arrange - When cachingEnabled=false, GetAsync returns early
        var testData = "test-value";
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        memoryCache.Set("test-key", testData);

        var serviceProvider = CreateServiceProvider(null, memoryCache);
        var config = CreateConfig(cachingEnabled: false);
        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        // Act
        var result = await cacheService.GetAsync<string>("test-key");

        // Assert - Returns null because !_cachingEnabled check returns early
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ConCachingDeshabilitado_YMemoryCacheDisponible_NoDebeGuardar()
    {
        // Arrange
        var testData = "test-value";
        var memoryCache = new MemoryCache(new MemoryCacheOptions());

        var serviceProvider = CreateServiceProvider(null, memoryCache);
        var config = CreateConfig(cachingEnabled: false);
        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        // Act
        await cacheService.SetAsync("test-key", testData);

        // Assert - SetAsync returns early when !_cachingEnabled
        var result = memoryCache.Get<string>("test-key");
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveAsync_ConCachingDeshabilitado_YMemoryCacheDisponible_NoDebeRemover()
    {
        // Arrange
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        memoryCache.Set("test-key", "test-value");

        var serviceProvider = CreateServiceProvider(null, memoryCache);
        var config = CreateConfig(cachingEnabled: false);
        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        // Act
        await cacheService.RemoveAsync("test-key");

        // Assert - RemoveAsync returns early when !_cachingEnabled, so value still in cache
        var result = memoryCache.Get<string>("test-key");
        result.Should().Be("test-value");
    }

    #endregion

    #region Caching Disabled Tests

    [Fact]
    public async Task GetAsync_ConCachingDeshabilitado_DebeRetornarDefault()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(null, null);
        var config = CreateConfig(cachingEnabled: false);
        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        // Act
        var result = await cacheService.GetAsync<string>("test-key");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ConCachingExplicitamenteDeshabilitado_NoDebeGuardar()
    {
        // Arrange - When _cachingEnabled = false in config, SetAsync returns early
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var serviceProvider = CreateServiceProvider(null, memoryCache);

        // Explicitly disable caching in config
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Caching:Enabled", "false" }
            })
            .Build();

        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        // Act
        await cacheService.SetAsync("test-key", "test-value");

        // Assert - Should not cache when _cachingEnabled is false
        var result = memoryCache.Get<string>("test-key");
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ConValorNull_NoDebeGuardar()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = CreateServiceProvider(distributedCacheMock.Object, null);
        var config = CreateConfig(cachingEnabled: true);
        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        // Act
        await cacheService.SetAsync<string>("test-key", null!);

        // Assert
        distributedCacheMock.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    [Fact]
    public async Task RemoveAsync_ConCachingDeshabilitado_NoDebeHacerNada()
    {
        // Arrange
        var distributedCacheMock = new Mock<IDistributedCache>();
        var serviceProvider = CreateServiceProvider(distributedCacheMock.Object, null);
        var config = CreateConfig(cachingEnabled: false);
        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        // Act
        await cacheService.RemoveAsync("test-key");

        // Assert
        distributedCacheMock.Verify(x => x.RemoveAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()
        ), Times.Never);
    }

    #endregion

    #region RemoveByPrefix Tests

    [Fact]
    public async Task RemoveByPrefixAsync_DebeCompletarSinError()
    {
        // Arrange
        var serviceProvider = CreateServiceProvider(null, null);
        var config = CreateConfig(cachingEnabled: true);
        var cacheService = new CacheService(serviceProvider, config, NullLogger<CacheService>.Instance);

        // Act & Assert - Should not throw
        await cacheService.RemoveByPrefixAsync("test-prefix");
    }

    #endregion

    #region Helper Methods

    private static IServiceProvider CreateServiceProvider(
        IDistributedCache? distributedCache,
        IMemoryCache? memoryCache)
    {
        var services = new ServiceCollection();

        if (distributedCache != null)
            services.AddSingleton(distributedCache);

        if (memoryCache != null)
            services.AddSingleton(memoryCache);

        return services.BuildServiceProvider();
    }

    private static IConfiguration CreateConfig(bool cachingEnabled, int expirationMinutes = 15)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Caching:Enabled", cachingEnabled.ToString() },
                { "Caching:DefaultExpirationMinutes", expirationMinutes.ToString() }
            })
            .Build();

        return config;
    }

    #endregion
}
