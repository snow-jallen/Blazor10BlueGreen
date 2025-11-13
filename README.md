# Blazor10BlueGreen
Can we do blue green deploys with blazor state persistence?

## Yes! ✅

This repository demonstrates a .NET 9 Blazor Server application with circuit state persistence that enables seamless blue-green deployments with zero state loss.

## What's Inside

A complete proof-of-concept Blazor Server application (`BlazorStateApp/`) that:

- ✅ Persists component state automatically using localStorage-based session IDs
- ✅ Survives server restarts without losing user state
- ✅ Enables transparent blue-green deployments
- ✅ Uses file-based storage (easily replaceable with Redis/Database for production)

## How It Works

1. **Session Persistence**: Each user gets a unique session ID stored in browser localStorage
2. **State Tracking**: Component state is automatically saved with each change
3. **Graceful Shutdown**: On shutdown, all active circuits save their state
4. **Automatic Recovery**: On reconnection, state is restored from the session ID

## Quick Start

```bash
cd BlazorStateApp
dotnet run
```

Navigate to `http://localhost:5000/counter` and try:
1. Increment the counter several times
2. Note your session ID
3. Stop the server (Ctrl+C)
4. Restart the server
5. Refresh the page - your counter value is restored! 🎉

## Architecture

See `BlazorStateApp/README.md` for detailed architecture documentation and production deployment guidelines.

## Key Components

- `ICircuitStateService` - State persistence interface
- `SessionStateManager` - Browser session tracking with localStorage
- `FileBasedCircuitStateService` - File-based storage implementation
- `StateCircuitHandler` - Circuit lifecycle event tracking
- `StatePreservationHostedService` - Graceful shutdown handler

## Production Considerations

For production deployments, replace the file-based storage with:
- Redis (recommended)
- Azure Cache for Redis
- SQL Database
- Cosmos DB

See the detailed README in `BlazorStateApp/` for more information.

