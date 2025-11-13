using Microsoft.JSInterop;

namespace BlazorStateApp.Services;

/// <summary>
/// Manages user session persistence across server restarts
/// </summary>
public class SessionStateManager
{
    private readonly IJSRuntime _jsRuntime;
    private readonly ICircuitStateService _stateService;
    private readonly ILogger<SessionStateManager> _logger;
    private string? _sessionId;

    public SessionStateManager(
        IJSRuntime jsRuntime,
        ICircuitStateService stateService,
        ILogger<SessionStateManager> logger)
    {
        _jsRuntime = jsRuntime;
        _stateService = stateService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the persistent session ID from browser storage
    /// </summary>
    public async Task<string> GetSessionIdAsync()
    {
        if (_sessionId == null)
        {
            try
            {
                _sessionId = await _jsRuntime.InvokeAsync<string>("blazorSession.getSessionId");
                _logger.LogInformation("Session ID retrieved: {SessionId}", _sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving session ID from browser");
                _sessionId = Guid.NewGuid().ToString();
            }
        }
        return _sessionId;
    }

    /// <summary>
    /// Saves component state with session-based key
    /// </summary>
    public async Task SaveComponentStateAsync<T>(string componentKey, T state) where T : class
    {
        var sessionId = await GetSessionIdAsync();
        var stateDict = new Dictionary<string, object>
        {
            [componentKey] = state
        };
        await _stateService.SaveStateAsync(sessionId, stateDict);
        _logger.LogInformation("Saved state for component {ComponentKey} with session {SessionId}", 
            componentKey, sessionId);
    }

    /// <summary>
    /// Loads component state using session-based key
    /// </summary>
    public async Task<T?> LoadComponentStateAsync<T>(string componentKey) where T : class
    {
        var sessionId = await GetSessionIdAsync();
        var circuitState = await _stateService.LoadStateAsync(sessionId);
        
        if (circuitState != null && circuitState.TryGetValue(componentKey, out var stateObj))
        {
            if (stateObj is System.Text.Json.JsonElement jsonElement)
            {
                var json = jsonElement.GetRawText();
                var state = System.Text.Json.JsonSerializer.Deserialize<T>(json);
                _logger.LogInformation("Loaded state for component {ComponentKey} with session {SessionId}", 
                    componentKey, sessionId);
                return state;
            }
        }
        
        _logger.LogInformation("No saved state found for component {ComponentKey} with session {SessionId}", 
            componentKey, sessionId);
        return null;
    }

    /// <summary>
    /// Clears the session
    /// </summary>
    public async Task ClearSessionAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("blazorSession.clearSessionId");
            _sessionId = null;
            _logger.LogInformation("Session cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing session");
        }
    }
}
