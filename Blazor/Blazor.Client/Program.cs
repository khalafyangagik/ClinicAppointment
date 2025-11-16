using Blazored.LocalStorage;
using ClinicClient.Auth;
using ClinicClient.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7057/")
});

// Local storage for JWT
builder.Services.AddBlazoredLocalStorage();

// Auth state
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// API services
builder.Services.AddScoped<ClinicApi>();
builder.Services.AddScoped<AuthApi>();

await builder.Build().RunAsync();


