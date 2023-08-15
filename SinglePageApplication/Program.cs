using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SinglePageApplication;
using SinglePageApplication.Handlers;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("Api", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://localhost:7001");
}).AddHttpMessageHandler<ApiAuthorizationMessageHandler>();

builder.Services.AddOidcAuthentication(remoteAuthenticationOptions =>
{
    remoteAuthenticationOptions.ProviderOptions.Authority = builder.Configuration[""];
    remoteAuthenticationOptions.ProviderOptions.ClientId = builder.Configuration[""];
    remoteAuthenticationOptions.ProviderOptions.ResponseType = "code";
    remoteAuthenticationOptions.ProviderOptions.DefaultScopes.Add("https://www.example.com/api");
});

builder.Services.AddScoped<ApiAuthorizationMessageHandler>();

await builder.Build().RunAsync();