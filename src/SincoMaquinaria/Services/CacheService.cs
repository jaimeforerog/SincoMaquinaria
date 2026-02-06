using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace SincoMaquinaria.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}

public class CacheService : ICacheService
{
    private readonly IDistributedCache? _distributedCache;
    private readonly IMemoryCache? _memoryCache;
    private readonly IConfiguration _configuration;
    private readonly bool _cachingEnabled;

    public CacheService(
        IServiceProvider serviceProvider,
        IConfiguration configuration)
    {
        _configuration = configuration;
        _cachingEnabled = configuration.GetValue<bool>("Caching:Enabled", false);

        if (_cachingEnabled)
        {
            _distributedCache = serviceProvider.GetService<IDistributedCache>();
        }
        else
        {
            _memoryCache = serviceProvider.GetService<IMemoryCache>();
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (!_cachingEnabled || (_distributedCache == null && _memoryCache == null))
            return default;

        if (_distributedCache != null)
        {
            var data = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (data == null) return default;
            return JsonSerializer.Deserialize<T>(data);
        }
        else if (_memoryCache != null)
        {
            return _memoryCache.Get<T>(key);
        }

        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        if (!_cachingEnabled || value == null) return;

        var defaultExpiration = TimeSpan.FromMinutes(
            _configuration.GetValue<int>("Caching:DefaultExpirationMinutes", 15));
        var finalExpiration = expiration ?? defaultExpiration;

        if (_distributedCache != null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = finalExpiration
            };
            var serialized = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, serialized, options, cancellationToken);
        }
        else if (_memoryCache != null)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = finalExpiration
            };
            _memoryCache.Set(key, value, options);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!_cachingEnabled) return;

        if (_distributedCache != null)
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }
        else if (_memoryCache != null)
        {
            _memoryCache.Remove(key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // Redis: Usar KEYS o SCAN (no implementado por ahora - require StackExchange.Redis directamente)
        // MemoryCache: No soportado fácilmente
        // Por ahora, solo invalidar keys conocidas manualmente
        await Task.CompletedTask;
    }
}
