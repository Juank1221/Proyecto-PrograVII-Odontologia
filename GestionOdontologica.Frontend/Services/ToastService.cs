namespace GestionOdontologica.Frontend.Services;

public enum ToastTipo
{
    Exito,
    Error,
    Info
}

public sealed class ToastMessage
{
    public Guid Id { get; } = Guid.NewGuid();

    public required string Titulo { get; init; }

    public required string Mensaje { get; init; }

    public ToastTipo Tipo { get; init; }

    public int DuracionMs { get; init; } = 4500;

    public bool Saliendo { get; set; }

    public string ClaseCss => Tipo switch
    {
        ToastTipo.Exito => "toast--success",
        ToastTipo.Error => "toast--error",
        _ => "toast--info"
    };
}

/// <summary>
/// Cola de notificaciones temporales. El contenedor visual (ToastContainer)
/// se suscribe a OnChange y se encarga de pintarlas.
/// </summary>
public sealed class ToastService
{
    private const int Maximo = 4;
    private const int DuracionSalidaMs = 180;

    private readonly List<ToastMessage> _toasts = new();

    public event Action? OnChange;

    public IReadOnlyList<ToastMessage> Toasts => _toasts;

    public void Mostrar(
        string titulo,
        string mensaje,
        ToastTipo tipo = ToastTipo.Exito,
        int duracionMs = 4500)
    {
        while (_toasts.Count >= Maximo)
        {
            _toasts.RemoveAt(0);
        }

        var toast = new ToastMessage
        {
            Titulo = titulo,
            Mensaje = mensaje,
            Tipo = tipo,
            DuracionMs = duracionMs
        };

        _toasts.Add(toast);
        Notificar();

        _ = CerrarDespuesAsync(toast, duracionMs);
    }

    public void Cerrar(ToastMessage toast)
    {
        if (toast.Saliendo || !_toasts.Contains(toast))
        {
            return;
        }

        toast.Saliendo = true;
        Notificar();
        _ = RetirarDespuesAsync(toast);
    }

    private async Task CerrarDespuesAsync(ToastMessage toast, int duracionMs)
    {
        await Task.Delay(duracionMs);
        Cerrar(toast);
    }

    private async Task RetirarDespuesAsync(ToastMessage toast)
    {
        await Task.Delay(DuracionSalidaMs);

        if (_toasts.Remove(toast))
        {
            Notificar();
        }
    }

    private void Notificar() => OnChange?.Invoke();
}
