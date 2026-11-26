using System.Text.Json;
using GestionOdontologica.Frontend.Models;
using Microsoft.JSInterop;

namespace GestionOdontologica.Frontend.Services;

/// <summary>
/// Conserva la sesión del usuario en localStorage. Guarda los datos públicos
/// que devuelve el login, el token de API y los códigos de permisos. No
/// almacena contraseña ni hash.
/// </summary>
public sealed class SesionService
{
    private const string ClaveAlmacenamiento = "odontocare.sesion.v1";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IJSRuntime _js;

    private Task? _inicializacion;

    public SesionService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>Se dispara cuando la sesión se carga, inicia o cierra.</summary>
    public event Action? OnChange;

    public SesionUsuario? Usuario { get; private set; }

    public bool Inicializada { get; private set; }

    public bool EsAutenticado => Usuario is { } usuario && usuario.EstaVigente;

    /// <summary>Token de API vigente o cadena vacía si no hay sesión.</summary>
    public string Token => Usuario?.Token ?? string.Empty;

    /// <summary>Códigos de permisos de la sesión actual.</summary>
    public IReadOnlyList<string> Permisos => Usuario?.Permisos ?? new List<string>();

    /// <summary>Comprueba si la sesión incluye un permiso concreto.</summary>
    public bool TienePermiso(string codigo) =>
        Usuario?.TienePermiso(codigo) ?? false;

    /// <summary>
    /// Carga la sesión una sola vez. Si expiró, se elimina del almacenamiento.
    /// </summary>
    public Task InicializarAsync()
    {
        return _inicializacion ??= CargarAsync();
    }

    public async Task IniciarSesionAsync(LoginResponse respuesta)
    {
        ArgumentNullException.ThrowIfNull(respuesta);

        Usuario = new SesionUsuario
        {
            IdUsuario = respuesta.IdUsuario,
            NombreCompleto = respuesta.NombreCompleto,
            NombreUsuario = respuesta.NombreUsuario,
            Correo = respuesta.Correo,
            Rol = respuesta.Rol,
            ExpiraEn = NormalizarUtc(respuesta.ExpiraEn),
            Token = respuesta.Token,
            Permisos = respuesta.Permisos?
                .Where(permiso => !string.IsNullOrWhiteSpace(permiso))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>()
        };

        Inicializada = true;
        _inicializacion = Task.CompletedTask;

        await GuardarAsync(Usuario);
        Notificar();
    }

    public async Task CerrarSesionAsync()
    {
        Usuario = null;
        Inicializada = true;
        _inicializacion = Task.CompletedTask;

        await EliminarAsync();
        Notificar();
    }

    private async Task CargarAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", ClaveAlmacenamiento);

            if (!string.IsNullOrWhiteSpace(json))
            {
                var sesion = JsonSerializer.Deserialize<SesionUsuario>(json, JsonOptions);

                if (sesion is not null && sesion.IdUsuario > 0)
                {
                    sesion.ExpiraEn = NormalizarUtc(sesion.ExpiraEn);
                    sesion.Permisos ??= new List<string>();

                    // Las sesiones creadas antes de la autorización por permisos
                    // no tienen token: deben volver a iniciar sesión.
                    if (sesion.EstaVigente && !string.IsNullOrWhiteSpace(sesion.Token))
                    {
                        Usuario = sesion;
                    }
                    else
                    {
                        await EliminarAsync();
                    }
                }
            }
        }
        catch (JSException)
        {
            // Si localStorage no está disponible la app sigue funcionando sin
            // persistencia; el usuario simplemente tendrá que iniciar sesión
            // otra vez al recargar.
        }
        catch (JsonException)
        {
            await EliminarAsync();
        }
        finally
        {
            Inicializada = true;
        }

        Notificar();
    }

    private async Task GuardarAsync(SesionUsuario sesion)
    {
        try
        {
            var json = JsonSerializer.Serialize(sesion, JsonOptions);
            await _js.InvokeVoidAsync("localStorage.setItem", ClaveAlmacenamiento, json);
        }
        catch (JSException)
        {
            // La sesión permanece en memoria durante la pestaña actual.
        }
    }

    private async Task EliminarAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", ClaveAlmacenamiento);
        }
        catch (JSException)
        {
            // Nada que hacer: no hay almacenamiento disponible.
        }
    }

    private static DateTime NormalizarUtc(DateTime valor)
    {
        return valor.Kind switch
        {
            DateTimeKind.Utc => valor,
            DateTimeKind.Local => valor.ToUniversalTime(),
            _ => DateTime.SpecifyKind(valor, DateTimeKind.Utc)
        };
    }

    private void Notificar() => OnChange?.Invoke();
}
