using GestionOdontologica.Api.Data;
using GestionOdontologica.Api.Infrastructure;
using GestionOdontologica.Api.Notifications;
using GestionOdontologica.Api.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Valida el catalogo de permisos antes de construir la aplicacion: si hay
// codigos duplicados o inconsistentes con las politicas, falla al arrancar.
PermisosCatalogo.Validar();

// ---------------------------------------------------------------------------
// Servicios de la API
// ---------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gestión Odontológica API",
        Version = "v1",
        Description = "Backend de gestión de usuarios, autenticación y recuperación de contraseña. Toda la persistencia usa procedimientos almacenados."
    });
});

// El factory encapsula la creación de OracleConnection y la cadena de conexión.
builder.Services.AddSingleton<IOracleConnectionFactory, OracleConnectionFactory>();

// Repositorios: toda la persistencia pasa por procedimientos almacenados.
builder.Services.AddScoped<IRolRepository, RolRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IPacienteRepository, PacienteRepository>();
builder.Services.AddScoped<IAntecedenteRepository, AntecedenteRepository>();
builder.Services.AddScoped<IOdontologoRepository, OdontologoRepository>();
builder.Services.AddScoped<IHorarioOdontologoRepository, HorarioOdontologoRepository>();
builder.Services.AddScoped<ITratamientoRepository, TratamientoRepository>();
builder.Services.AddScoped<IOdontologoTratamientoRepository, OdontologoTratamientoRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IPermisoRepository, PermisoRepository>();

// Inicializacion automatica del catalogo de permisos y de las asignaciones
// por rol. Solo hace DML sobre PERMISOS y ROL_PERMISOS; se puede desactivar
// con Permisos:Seed=false (por ejemplo en QA).
builder.Services.AddHostedService<PermisoSeederHostedService>();

// Servicio de correo configurable para el flujo de recuperación de contraseña.
// Con Email:Habilitado=false la API registra un aviso y no intenta enviar.
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

// ---------------------------------------------------------------------------
// Seguridad: token de sesión firmado + autorización basada en permisos (RBAC).
// Los permisos se leen de Oracle en cada solicitud, de modo que los cambios de
// asignación se aplican de inmediato. La política fallback exige autenticación:
// todo endpoint queda protegido salvo que declare [AllowAnonymous].
// ---------------------------------------------------------------------------
builder.Services.AddDataProtection()
    .SetApplicationName("GestionOdontologica.Api");

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ISesionTokenService, SesionTokenService>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services
    .AddAuthentication(SesionAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, SesionAuthenticationHandler>(
        SesionAuthenticationHandler.SchemeName,
        _ => { });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(SesionAuthenticationHandler.SchemeName)
        .RequireAuthenticatedUser()
        .Build();

    foreach (var codigo in PermisoCodigos.Todos)
    {
        options.AddPolicy(codigo, policy => policy
            .AddAuthenticationSchemes(SesionAuthenticationHandler.SchemeName)
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(codigo)));
    }
});

// CORS: permite que el frontend estático consuma la API desde otro origen.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

if (allowedOrigins is null || allowedOrigins.Length == 0)
{
    allowedOrigins = new[]
    {
        "http://localhost:5500",
        "http://127.0.0.1:5500",
        "http://localhost:5501",
        "http://127.0.0.1:5501"
    };
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Manejo global de errores no controlados (ProblemDetails).
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Debe ir antes de los demás middlewares para capturar sus excepciones.
app.UseExceptionHandler();

// MODIFICACIÓN: Swagger habilitado para cualquier entorno (Development y Production)
// para que puedas visualizarlo y probarlo sin inconvenientes durante la presentación.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gestión Odontológica API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors("Frontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
