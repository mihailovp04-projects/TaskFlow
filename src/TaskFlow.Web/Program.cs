using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TaskFlow.Web;
using TaskFlow.Web.Auth;
using TaskFlow.Web.Services;
using TaskFlow.Web.State;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl is missing in wwwroot/appsettings.json.");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Неделя 6: данные задач пока из памяти (реальный HttpTaskApiClient появится на неделе 8)
builder.Services.AddSingleton<ITaskApiClient, FakeTaskApiClient>();

// Неделя 7: JS interop и состояние
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<BrowserDialogService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<ThemeState>();

// Неделя 7: аутентификация
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<TokenAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<TokenAuthStateProvider>());

// Отдельный HttpClient для /api/auth: базовый адрес API, без заголовка Authorization
builder.Services.AddScoped(sp => new AuthService(
    new HttpClient { BaseAddress = new Uri(apiBaseUrl) },
    sp.GetRequiredService<TokenAuthStateProvider>()));

await builder.Build().RunAsync();
