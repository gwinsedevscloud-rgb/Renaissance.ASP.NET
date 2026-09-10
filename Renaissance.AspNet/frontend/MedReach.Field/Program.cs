using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MedReach.Field;
using MedReach.Field.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["ApiBaseUrl"]
              ?? "http://localhost:5280/";
if (!apiBase.EndsWith('/'))
{
    apiBase += "/";
}

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBase) });
builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddScoped<DeviceIdentityService>();
builder.Services.AddScoped<ConnectivityService>();
builder.Services.AddScoped<FieldApiClient>();
builder.Services.AddScoped<OutreachCacheService>();
builder.Services.AddScoped<FieldQueueService>();
builder.Services.AddScoped<SyncOrchestrator>();
builder.Services.AddScoped<BackgroundSyncWorker>();
builder.Services.AddScoped<NotificationService>();

await builder.Build().RunAsync();
