namespace BackendDotnetLayer.Services;

public sealed class SemanticRouterInitializationService : IHostedService
{
    private readonly SemanticRoutingService _semanticRoutingService;

    public SemanticRouterInitializationService(SemanticRoutingService semanticRoutingService)
    {
        _semanticRoutingService = semanticRoutingService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _semanticRoutingService.EnsureInitializedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
