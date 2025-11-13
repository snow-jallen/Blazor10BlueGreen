using Microsoft.AspNetCore.Components.Server.Circuits;
using System.Text.Json;

namespace BlazorStateApp.Services;

/// <summary>
/// Service for managing component state within a circuit
/// </summary>
public class ComponentStateManager
{
    private readonly ICircuitStateService _stateService;
    private readonly Circuit? _circuit;
    private readonly ILogger<ComponentStateManager> _logger;
    private readonly Dictionary<string, object> _componentStates = new();
    private readonly object _lock = new();

    public ComponentStateManager(
        ICircuitStateService stateService,
        ILogger<ComponentStateManager> logger,
        Circuit? circuit = null)
    {
        _stateService = stateService;
        _logger = logger;
        _circuit = circuit;
    }

    /// <summary>
    /// Gets the circuit ID for this manager
    /// </summary>
    public string? CircuitId => _circuit?.Id;

    /// <summary>
    /// Loads state for a component
    /// </summary>
    public async Task<T?> LoadComponentStateAsync<T>(string componentKey) where T : class
    {
        if (string.IsNullOrEmpty(CircuitId))
        {
            _logger.LogWarning("Cannot load state - no circuit ID available");
            return null;
        }

        try
        {
            var circuitState = await _stateService.LoadStateAsync(CircuitId);
            
            if (circuitState != null && circuitState.TryGetValue(componentKey, out var stateObj))
            {
                // Convert JsonElement to the target type
                if (stateObj is JsonElement jsonElement)
                {
                    var json = jsonElement.GetRawText();
                    var state = JsonSerializer.Deserialize<T>(json);
                    _logger.LogInformation("Loaded state for component {ComponentKey} in circuit {CircuitId}", 
                        componentKey, CircuitId);
                    return state;
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading state for component {ComponentKey}", componentKey);
            return null;
        }
    }

    /// <summary>
    /// Saves state for a component
    /// </summary>
    public async Task SaveComponentStateAsync<T>(string componentKey, T state) where T : class
    {
        if (string.IsNullOrEmpty(CircuitId))
        {
            _logger.LogWarning("Cannot save state - no circuit ID available");
            return;
        }

        try
        {
            lock (_lock)
            {
                _componentStates[componentKey] = state;
            }

            // Get all component states for this circuit
            Dictionary<string, object> allStates;
            lock (_lock)
            {
                allStates = new Dictionary<string, object>(_componentStates);
            }

            await _stateService.SaveStateAsync(CircuitId, allStates);
            _logger.LogInformation("Saved state for component {ComponentKey} in circuit {CircuitId}", 
                componentKey, CircuitId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving state for component {ComponentKey}", componentKey);
        }
    }

    /// <summary>
    /// Updates a single value in the component state and saves it
    /// </summary>
    public Task UpdateStateValueAsync<T>(string componentKey, string propertyName, T value)
    {
        lock (_lock)
        {
            if (!_componentStates.ContainsKey(componentKey))
            {
                _componentStates[componentKey] = new Dictionary<string, object>();
            }

            var componentState = _componentStates[componentKey] as Dictionary<string, object>;
            if (componentState != null)
            {
                componentState[propertyName] = value!;
            }
        }

        return SaveAllAsync();
    }

    /// <summary>
    /// Saves all component states for this circuit
    /// </summary>
    private async Task SaveAllAsync()
    {
        if (string.IsNullOrEmpty(CircuitId))
        {
            return;
        }

        Dictionary<string, object> allStates;
        lock (_lock)
        {
            allStates = new Dictionary<string, object>(_componentStates);
        }

        await _stateService.SaveStateAsync(CircuitId, allStates);
    }
}
