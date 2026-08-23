using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SaborByte.Web.Api;
using SaborByte.Web.Central;

CulturaApp.Aplicar();

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddSingleton<SesionCliente>();
builder.Services.AddScoped(sp =>
{
    var handler = new SesionExpiradaHandler(sp.GetRequiredService<SesionCliente>(), sp.GetRequiredService<NavigationManager>())
    {
        InnerHandler = new HttpClientHandler()
    };
    return new HttpClient(handler) { BaseAddress = new Uri(apiBaseUrl) };
});
builder.Services.AddScoped<SaborByteApiClient>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
