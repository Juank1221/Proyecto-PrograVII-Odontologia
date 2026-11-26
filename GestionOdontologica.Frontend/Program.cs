using GestionOdontologica.Frontend;
using GestionOdontologica.Frontend.Models;
using GestionOdontologica.Frontend.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// La URL se configura en wwwroot/appsettings.json (Api:BaseUrl). El fallback
// mantiene la aplicación operativa si el archivo no está disponible.
var apiBaseUrl = builder.Configuration["Api:BaseUrl"];

if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    apiBaseUrl = "http://localhost:5080/api/";
}

apiBaseUrl = apiBaseUrl.TrimEnd('/') + "/";

if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri))
{
    apiUri = new Uri("http://localhost:5080/api/");
}

// ---------------------------------------------------------------------------
// Autorización del cliente. Cada permiso del backend se registra como política
// y se resuelve con el claim "permiso" que construye el proveedor de estado.
// El backend siempre revalida; estas políticas solo controlan la navegación y
// la visibilidad de las acciones.
// ---------------------------------------------------------------------------
builder.Services.AddAuthorizationCore(options =>
{
    foreach (var codigo in PermisoCodigos.Todos)
    {
        options.AddPolicy(codigo, policy =>
            policy.RequireClaim("permiso", codigo));
    }
});

builder.Services.AddSingleton<ApiStatusService>();
builder.Services.AddSingleton<ToastService>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddScoped<SesionService>();
builder.Services.AddScoped<SesionAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    servicios => servicios.GetRequiredService<SesionAuthenticationStateProvider>());
builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<UsuariosApiService>();
builder.Services.AddScoped<PacientesApiService>();
builder.Services.AddScoped<OdontologosApiService>();
builder.Services.AddScoped<TratamientosApiService>();
builder.Services.AddScoped<RolesApiService>();
builder.Services.AddScoped<PermisosApiService>();
builder.Services.AddScoped<ProductosApiService>();
builder.Services.AddScoped<CategoriasApiService>();

// HttpClient con el token de sesión adjunto en Authorization: Bearer.
builder.Services.AddScoped<AuthHeaderHandler>();
builder.Services.AddScoped(servicios =>
{
    var handler = servicios.GetRequiredService<AuthHeaderHandler>();
    handler.InnerHandler = new HttpClientHandler();

    return new HttpClient(handler)
    {
        BaseAddress = apiUri,
        Timeout = TimeSpan.FromSeconds(30)
    };
});

await builder.Build().RunAsync();
