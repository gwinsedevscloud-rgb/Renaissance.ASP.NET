using Renaissance.Web.Components;
using Renaissance.Web.Middleware;
using Renaissance.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");

builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped(sp =>
{
    var auth = sp.GetRequiredService<AuthStateService>();
    var handler = new AuthDelegatingHandler(auth)
    {
        InnerHandler = new HttpClientHandler()
    };
    var http = new HttpClient(handler)
    {
        BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/")
    };
    return new RenaissanceApiClient(http);
});

builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<ReferralRealtimeService>();
builder.Services.AddScoped<HospitalModuleStateService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
