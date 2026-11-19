using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TaskFlow.Web;
using TaskFlow.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Неделя 6: данные в памяти. Неделя 8: эта строка заменяется на регистрацию HttpTaskApiClient.
builder.Services.AddSingleton<ITaskApiClient, FakeTaskApiClient>();

await builder.Build().RunAsync();
