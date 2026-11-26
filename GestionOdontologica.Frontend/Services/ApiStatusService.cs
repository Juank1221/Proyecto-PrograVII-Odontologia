namespace GestionOdontologica.Frontend.Services;

public enum EstadoApi
{
    Desconocido,
    Cargando,
    EnLinea,
    Error,
    SinConexion
}

/// <summary>
/// Estado compartido de la conexión con la API. Lo actualiza la página de
/// usuarios y lo consumen el topbar y las tarjetas de resumen del layout.
/// </summary>
public sealed class ApiStatusService
{
    public event Action? OnChange;

    public EstadoApi Estado { get; private set; } = EstadoApi.Desconocido;

    public string ClaseCss => Estado switch
    {
        EstadoApi.Cargando => "is-loading",
        EstadoApi.EnLinea => "is-online",
        EstadoApi.Error => "is-error",
        EstadoApi.SinConexion => "is-offline",
        _ => "is-unknown"
    };

    public string Texto => Estado switch
    {
        EstadoApi.Cargando => "Conectando API...",
        EstadoApi.EnLinea => "API conectada",
        EstadoApi.Error => "API con error",
        EstadoApi.SinConexion => "API sin conexión",
        _ => "API sin consultar"
    };

    public string TextoMeta => Estado switch
    {
        EstadoApi.Cargando => "Conectando...",
        EstadoApi.EnLinea => "En línea",
        EstadoApi.Error => "Con errores",
        EstadoApi.SinConexion => "Sin conexión",
        _ => "Sin consultar"
    };

    public void Establecer(EstadoApi estado)
    {
        if (Estado == estado)
        {
            return;
        }

        Estado = estado;
        OnChange?.Invoke();
    }
}
