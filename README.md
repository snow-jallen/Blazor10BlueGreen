# Blazor10BlueGreen
Can we do blue green deploys with blazor state persistence?

## Yes! ✅

This repository demonstrates a **.NET 10** Blazor Server application using the new built-in `PersistentComponentState` APIs for state management during circuit reconnections and prerendering.

## What's Inside

A proof-of-concept Blazor Server application (`BlazorStateApp/`) built with .NET 10 that:

- ✅ Uses .NET 10's built-in `PersistentComponentState` service
- ✅ Demonstrates state persistence during circuit evictions
- ✅ Shows framework-level state management (no custom services needed)
- ✅ Simplifies state handling with built-in APIs

## .NET 10 State Persistence

.NET 10 introduces native Blazor state persistence APIs:
- `PersistentComponentState` - Framework service for state persistence
- `RegisterOnPersisting` - Hook into state save lifecycle
- `TryTakeFromJson` / `PersistAsJson` - Restore and save state

These APIs handle state during:
- **Circuit evictions** (inactivity, connection loss)
- **Prerendering** scenarios
- **Reconnection** events

## Quick Start

```bash
cd BlazorStateApp
dotnet run
```

Navigate to `http://localhost:5000/counter` to see state persistence in action.

## Architecture

The .NET 10 implementation uses built-in framework APIs instead of custom services:
- No custom SessionStateManager needed
- No JavaScript interop required
- Framework handles serialization automatically
- Built-in lifecycle hooks for state persistence

See `BlazorStateApp/README.md` for detailed documentation.

## Note on Blue-Green Deployments

The `PersistentComponentState` APIs in .NET 10 are designed for:
- Circuit reconnection scenarios
- Prerendering state transfer
- Handling temporary disconnections

For full blue-green deployment support across server restarts with session continuity, you would still need to implement cross-instance state storage (Redis, database, etc.) as shown in the earlier commits.


