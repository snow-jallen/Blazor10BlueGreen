using Microsoft.AspNetCore.Components.Server.Circuits;

namespace BlazorStateApp.Services;

/// <summary>
/// Handles circuit lifecycle events for state persistence
/// </summary>
public class StateCircuitHandler : CircuitHandler
{
    private readonly ICircuitStateService _stateService;
    private readonly ILogger<StateCircuitHandler> _logger;

    public StateCircuitHandler(
        ICircuitStateService stateService,
        ILogger<StateCircuitHandler> logger)
    {
        _stateService = stateService;
        _logger = logger;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId} opened", circuit.Id);
        return Task.CompletedTask;
    }

    public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId} closed", circuit.Id);
        
        // Clean up state when circuit closes normally
        try
        {
            await _stateService.RemoveStateAsync(circuit.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing state for circuit {CircuitId}", circuit.Id);
        }
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId} connection up", circuit.Id);
        return Task.CompletedTask;
    }

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Circuit {CircuitId} connection down", circuit.Id);
        return Task.CompletedTask;
    }
}
