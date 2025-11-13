using BlazorStateApp.Components;
using BlazorStateApp.Services;
using Microsoft.AspNetCore.Components.Server.Circuits;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register state persistence services for blue-green deployments
// These services enable cross-server state persistence using session IDs stored in browser localStorage
builder.Services.AddSingleton<ICircuitStateService, FileBasedCircuitStateService>();
builder.Services.AddScoped<ComponentStateManager>();
builder.Services.AddScoped<SessionStateManager>();
builder.Services.AddScoped<CircuitHandler, StateCircuitHandler>();
builder.Services.AddHostedService<StatePreservationHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
