using Microsoft.JSInterop;

namespace GestionOdontologica.Frontend.Services;

/// <summary>
/// Gestiona el tema claro/oscuro. La preferencia inicial se aplica desde
/// index.html antes de cargar Blazor (para evitar destellos) y se conserva en
/// localStorage; si no existe, se usa la preferencia del sistema operativo.
/// </summary>
public sealed class ThemeService
{
    private readonly IJSRuntime _js;

    private Task? _inicializacion;

    public ThemeService(IJSRuntime js)
    {
        _js = js;
    }

    /// <summary>Se dispara cuando cambia el tema.</summary>
    public event Action? OnChange;

    public bool ModoOscuro { get; private set; }

    public bool Inicializado { get; private set; }

    public Task InicializarAsync()
    {
        return _inicializacion ??= CargarAsync();
    }

    public async Task AlternarAsync()
    {
        await InicializarAsync();
        await EstablecerAsync(!ModoOscuro);
    }

    public async Task EstablecerAsync(bool oscuro)
    {
        ModoOscuro = oscuro;

        try
        {
            await _js.InvokeVoidAsync("odontocareTema.aplicar", oscuro);
        }
        catch (JSException)
        {
            // Sin JS disponible el atributo ya se aplicó desde index.html y la
            // interfaz sigue usando el tema visual cargado.
        }

        Inicializado = true;
        Notificar();
    }

    private async Task CargarAsync()
    {
        try
        {
            ModoOscuro = await _js.InvokeAsync<bool>("odontocareTema.estaOscuro");
        }
        catch (JSException)
        {
            ModoOscuro = false;
        }
        finally
        {
            Inicializado = true;
            Notificar();
        }
    }

    private void Notificar() => OnChange?.Invoke();
}
