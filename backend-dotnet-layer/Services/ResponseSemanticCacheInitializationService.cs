namespace BackendDotnetLayer.Services;

public sealed class ResponseSemanticCacheInitializationService : IHostedService
{
    private readonly ResponseSemanticCacheService _cacheService;

    public ResponseSemanticCacheInitializationService(ResponseSemanticCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public Task StartAsync(CancellationToken cancellationToken) =>
        _cacheService.EnsureInitializedAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
