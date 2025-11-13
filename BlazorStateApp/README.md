# Blazor Server State Persistence - Blue-Green Deployment POC

This is a proof-of-concept application demonstrating how to implement state persistence in .NET 9 Blazor Server applications to enable seamless blue-green deployments with zero state loss.

## Overview

In traditional Blazor Server deployments, when you deploy a new version of the application, active user circuits are terminated, causing users to lose their in-memory state. This POC demonstrates how to persist circuit state before shutdown and restore it when the new version starts, making the deployment transparent to users.

## Architecture

### Core Components

1. **ICircuitStateService** - Interface defining state persistence operations
2. **FileBasedCircuitStateService** - File-based implementation for POC (use Redis/Database in production)
3. **ComponentStateManager** - Scoped service for managing component-level state
4. **StateCircuitHandler** - Circuit lifecycle event handler
5. **StatePreservationHostedService** - Background service that saves all states during application shutdown

### How It Works

1. **State Tracking**: Components register their state with `ComponentStateManager`
2. **Automatic Persistence**: State is saved to storage whenever it changes
3. **Graceful Shutdown**: `StatePreservationHostedService` intercepts application shutdown and saves all active circuit states
4. **State Restoration**: When components initialize, they check for saved state and restore it automatically
5. **Circuit Tracking**: Each circuit is identified by a unique ID, allowing state to be associated with specific user sessions

## Running the Application

### Prerequisites
- .NET 9 SDK

### Build and Run

```bash
cd BlazorStateApp
dotnet build
dotnet run
```

Navigate to `http://localhost:5000` (or the port shown in the console)

## Testing Blue-Green Deployment

1. **Start the application**
   ```bash
   dotnet run
   ```

2. **Open the application** in your browser

3. **Navigate to the Counter page** (`/counter`)

4. **Increment the counter** several times (e.g., to 42)

5. **Note the Circuit ID** displayed on the page

6. **Simulate deployment**: Stop the server (Ctrl+C) and observe the logs showing state being saved

7. **Restart the server**
   ```bash
   dotnet run
   ```

8. **Refresh the browser** - Your counter value is restored! 🎉

## Key Features

### Automatic State Persistence
- State is saved on every change
- No manual save required (though manual save/load buttons are provided for demonstration)

### Graceful Shutdown
- Application shutdown triggers batch save of all active circuits
- Ensures no state is lost during deployment

### Transparent Recovery
- Components automatically load saved state on initialization
- Users don't notice the deployment happened

### Circuit-Based Isolation
- Each user session (circuit) maintains its own state
- State is isolated between users

## Configuration

State storage is configured in `appsettings.json`:

```json
{
  "StateStorage": {
    "Directory": "/tmp/blazor-state"
  }
}
```

## Production Considerations

This POC uses file-based storage for simplicity. For production use:

### Recommended Storage Options

1. **Redis** - Fast, distributed, persistent
   ```csharp
   public class RedisCircuitStateService : ICircuitStateService
   {
       private readonly IConnectionMultiplexer _redis;
       // Implementation using StackExchange.Redis
   }
   ```

2. **Azure Cache for Redis** - Managed Redis service
3. **SQL Database** - For durability and queryability
4. **Cosmos DB** - For global distribution

### Additional Considerations

1. **State Expiration**: Implement TTL to clean up old state
2. **State Versioning**: Handle schema changes between versions
3. **Compression**: Compress large state objects
4. **Security**: Encrypt sensitive state data
5. **Monitoring**: Track state persistence metrics
6. **Health Checks**: Verify storage availability

## Blue-Green Deployment Strategy

### Deployment Flow

1. **Green Environment (New)**
   - Deploy new version to green environment
   - New environment shares same state storage
   - Health checks pass

2. **Traffic Switch**
   - Load balancer switches traffic to green
   - Blue environment receives no new connections

3. **Graceful Shutdown**
   - Blue environment waits for idle connections
   - Saves all remaining circuit states
   - Shuts down

4. **State Recovery**
   - Users reconnect to green environment
   - State is automatically restored
   - No user impact

### Load Balancer Configuration

Configure connection draining:
- **Drain Timeout**: 30-60 seconds
- **Health Check**: Monitor application readiness
- **Session Affinity**: Not required (state is external)

## Code Structure

```
BlazorStateApp/
├── Components/
│   ├── Pages/
│   │   ├── Counter.razor      # Demo component with state
│   │   ├── Home.razor          # Landing page
│   │   └── Weather.razor       # Default component
│   └── Layout/
├── Services/
│   ├── ICircuitStateService.cs              # State service interface
│   ├── FileBasedCircuitStateService.cs      # File storage implementation
│   ├── ComponentStateManager.cs             # Component state helper
│   ├── StateCircuitHandler.cs               # Circuit lifecycle handler
│   └── StatePreservationHostedService.cs    # Shutdown handler
├── Program.cs              # Application entry point
└── appsettings.json        # Configuration

```

## API Reference

### ComponentStateManager

```csharp
// Save component state
await StateManager.SaveComponentStateAsync("MyComponent", stateObject);

// Load component state
var state = await StateManager.LoadComponentStateAsync<MyStateType>("MyComponent");
```

### ICircuitStateService

```csharp
// Save state for a circuit
await _stateService.SaveStateAsync(circuitId, stateDict);

// Load state for a circuit
var state = await _stateService.LoadStateAsync(circuitId);

// Save all active circuits (shutdown scenario)
await _stateService.SaveAllCircuitsAsync();
```

## Logging

The application logs state operations at Information level:

```
[INF] Circuit state service initialized. State directory: /tmp/blazor-state
[INF] Circuit abc123 opened
[INF] Saved state for component Counter in circuit abc123
[WRN] Application is stopping - saving all circuit states for blue-green deployment
[INF] Saved 5 circuits
[INF] Loaded state for component Counter in circuit abc123
```

## Limitations & Future Enhancements

### Current Limitations
- File-based storage (not suitable for multi-instance deployments)
- No state encryption
- No state versioning
- Basic error handling

### Potential Enhancements
- Distributed cache support (Redis)
- State compression
- State encryption
- State migration helpers
- Advanced monitoring and metrics
- State cleanup/garbage collection
- Multi-component state batching

## Contributing

This is a proof-of-concept. Feel free to extend it for your production needs!

## License

MIT License - Use freely in your projects

## References

- [Blazor Server Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Circuit Handler Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/signalr#circuit-handler)
- [Blue-Green Deployment Pattern](https://martinfowler.com/bliki/BlueGreenDeployment.html)
