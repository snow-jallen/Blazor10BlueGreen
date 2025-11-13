namespace BlazorStateApp.Services;

/// <summary>
/// Background service that handles state persistence during shutdown
/// </summary>
public class StatePreservationHostedService : IHostedService
{
    private readonly ICircuitStateService _stateService;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly ILogger<StatePreservationHostedService> _logger;

    public StatePreservationHostedService(
        ICircuitStateService stateService,
        IHostApplicationLifetime applicationLifetime,
        ILogger<StatePreservationHostedService> logger)
    {
        _stateService = stateService;
        _applicationLifetime = applicationLifetime;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("State preservation service started");
        
        // Register for application stopping event
        _applicationLifetime.ApplicationStopping.Register(OnApplicationStopping);
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("State preservation service stopped");
        return Task.CompletedTask;
    }

    private void OnApplicationStopping()
    {
        _logger.LogWarning("Application is stopping - saving all circuit states for blue-green deployment");
        
        try
        {
            // Save all active circuits before shutdown
            _stateService.SaveAllCircuitsAsync().GetAwaiter().GetResult();
            _logger.LogInformation("Successfully saved all circuit states before shutdown");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving circuit states during shutdown");
        }
    }
}
