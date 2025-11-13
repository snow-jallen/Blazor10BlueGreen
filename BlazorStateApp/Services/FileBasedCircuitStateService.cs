using System.Text.Json;

namespace BlazorStateApp.Services;

/// <summary>
/// File-based implementation of circuit state persistence for blue-green deployments
/// </summary>
public class FileBasedCircuitStateService : ICircuitStateService
{
    private readonly string _stateDirectory;
    private readonly ILogger<FileBasedCircuitStateService> _logger;
    private readonly Dictionary<string, Dictionary<string, object>> _activeCircuits = new();
    private readonly object _lock = new();

    public FileBasedCircuitStateService(
        IConfiguration configuration,
        ILogger<FileBasedCircuitStateService> logger)
    {
        _logger = logger;
        _stateDirectory = configuration.GetValue<string>("StateStorage:Directory") 
            ?? Path.Combine(Path.GetTempPath(), "blazor-state");
        
        // Ensure directory exists
        Directory.CreateDirectory(_stateDirectory);
        
        _logger.LogInformation("Circuit state service initialized. State directory: {Directory}", _stateDirectory);
    }

    public Task SaveStateAsync(string circuitId, Dictionary<string, object> state)
    {
        try
        {
            lock (_lock)
            {
                _activeCircuits[circuitId] = new Dictionary<string, object>(state);
            }

            var filePath = GetStateFilePath(circuitId);
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            
            File.WriteAllText(filePath, json);
            _logger.LogInformation("Saved state for circuit {CircuitId}", circuitId);
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving state for circuit {CircuitId}", circuitId);
            throw;
        }
    }

    public Task<Dictionary<string, object>?> LoadStateAsync(string circuitId)
    {
        try
        {
            var filePath = GetStateFilePath(circuitId);
            
            if (!File.Exists(filePath))
            {
                _logger.LogInformation("No saved state found for circuit {CircuitId}", circuitId);
                return Task.FromResult<Dictionary<string, object>?>(null);
            }

            var json = File.ReadAllText(filePath);
            var state = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            
            _logger.LogInformation("Loaded state for circuit {CircuitId}", circuitId);
            return Task.FromResult(state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading state for circuit {CircuitId}", circuitId);
            return Task.FromResult<Dictionary<string, object>?>(null);
        }
    }

    public Task RemoveStateAsync(string circuitId)
    {
        try
        {
            lock (_lock)
            {
                _activeCircuits.Remove(circuitId);
            }

            var filePath = GetStateFilePath(circuitId);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Removed state for circuit {CircuitId}", circuitId);
            }
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing state for circuit {CircuitId}", circuitId);
            throw;
        }
    }

    public async Task SaveAllCircuitsAsync()
    {
        _logger.LogInformation("Saving all active circuits...");
        
        Dictionary<string, Dictionary<string, object>> circuitsToSave;
        lock (_lock)
        {
            circuitsToSave = new Dictionary<string, Dictionary<string, object>>(_activeCircuits);
        }

        foreach (var kvp in circuitsToSave)
        {
            try
            {
                await SaveStateAsync(kvp.Key, kvp.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving circuit {CircuitId} during batch save", kvp.Key);
            }
        }
        
        _logger.LogInformation("Saved {Count} circuits", circuitsToSave.Count);
    }

    private string GetStateFilePath(string circuitId)
    {
        // Use a safe file name based on circuit ID
        var safeFileName = string.Join("_", circuitId.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_stateDirectory, $"{safeFileName}.json");
    }
}
