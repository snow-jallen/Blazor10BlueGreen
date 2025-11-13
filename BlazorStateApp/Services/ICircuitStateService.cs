namespace BlazorStateApp.Services;

/// <summary>
/// Interface for managing circuit state persistence
/// </summary>
public interface ICircuitStateService
{
    /// <summary>
    /// Saves the current circuit state
    /// </summary>
    Task SaveStateAsync(string circuitId, Dictionary<string, object> state);

    /// <summary>
    /// Loads circuit state if it exists
    /// </summary>
    Task<Dictionary<string, object>?> LoadStateAsync(string circuitId);

    /// <summary>
    /// Removes saved state for a circuit
    /// </summary>
    Task RemoveStateAsync(string circuitId);

    /// <summary>
    /// Saves all active circuits state (for shutdown scenarios)
    /// </summary>
    Task SaveAllCircuitsAsync();
}
