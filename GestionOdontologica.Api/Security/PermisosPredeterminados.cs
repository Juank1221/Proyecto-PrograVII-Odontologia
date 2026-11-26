using System.Globalization;
using System.Text;

namespace GestionOdontologica.Api.Security;

/// <summary>
/// Permisos de respaldo por rol. Se usan unicamente cuando la base de datos no
/// tiene asignaciones en ROL_PERMISOS (o no tiene las tablas del modulo). De
/// esta forma la aplicacion sigue siendo utilizable mientras el administrador
/// configura el catalogo real desde la pantalla de Permisos.
///
/// Los conjuntos replican las asignaciones iniciales documentadas para el
/// sistema y se dejan de usar automaticamente en cuanto el rol tiene al menos
/// un permiso asignado en Oracle.
///
/// La misma matriz la usa <c>PermisoSeederHostedService</c> para poblar
/// ROL_PERMISOS de forma aditiva e idempotente.
/// </summary>
public static class PermisosPredeterminados
{
    private static readonly IReadOnlyList<string> Inicio =
        new[] { PermisoCodigos.InicioVer };

    private static readonly IReadOnlyList<string> Administrador =
        PermisoCodigos.Todos;

    /// <summary>
    /// Recepcion: agenda, pacientes, pagos, facturacion y cuentas por cobrar.
    /// No administra seguridad ni inventario.
    /// </summary>
    private static readonly IReadOnlyList<string> Recepcionista = new[]
    {
        PermisoCodigos.InicioVer,
        PermisoCodigos.PacientesVer,
        PermisoCodigos.PacientesCrear,
        PermisoCodigos.PacientesEditar,
        PermisoCodigos.PacientesExpediente,
        PermisoCodigos.OdontologosVer,
        PermisoCodigos.TratamientosVer,
        PermisoCodigos.CitasVer,
        PermisoCodigos.CitasCrear,
        PermisoCodigos.CitasEditar,
        PermisoCodigos.CitasCancelar,
        PermisoCodigos.PagosVer,
        PermisoCodigos.PagosCrear,
        PermisoCodigos.FacturacionVer,
        PermisoCodigos.FacturacionEmitir,
        PermisoCodigos.CuentasPorCobrarVer,
        PermisoCodigos.CuentasPorCobrarGestionar,
        PermisoCodigos.PromocionesVer,
        PermisoCodigos.ReportesVer
    };

    /// <summary>Perfil clinico: consulta pacientes, expediente, agenda y tratamientos.</summary>
    private static readonly IReadOnlyList<string> Odontologo = new[]
    {
        PermisoCodigos.InicioVer,
        PermisoCodigos.PacientesVer,
        PermisoCodigos.PacientesExpediente,
        PermisoCodigos.OdontologosVer,
        PermisoCodigos.TratamientosVer,
        PermisoCodigos.CitasVer,
        PermisoCodigos.ReportesVer
    };

    /// <summary>Asistente clinico: apoyo en pacientes, agenda y horarios.</summary>
    private static readonly IReadOnlyList<string> AsistenteOdontologia = new[]
    {
        PermisoCodigos.InicioVer,
        PermisoCodigos.PacientesVer,
        PermisoCodigos.PacientesCrear,
        PermisoCodigos.PacientesEditar,
        PermisoCodigos.PacientesExpediente,
        PermisoCodigos.OdontologosVer,
        PermisoCodigos.OdontologosHorarios,
        PermisoCodigos.TratamientosVer,
        PermisoCodigos.CitasVer,
        PermisoCodigos.CitasCrear,
        PermisoCodigos.CitasEditar,
        PermisoCodigos.ReportesVer
    };

    /// <summary>Inventario / bodega: control total de productos y movimientos.</summary>
    private static readonly IReadOnlyList<string> InventarioBodeguero = new[]
    {
        PermisoCodigos.InicioVer,
        PermisoCodigos.InventarioVer,
        PermisoCodigos.InventarioCrear,
        PermisoCodigos.InventarioEditar,
        PermisoCodigos.InventarioDesactivar,
        PermisoCodigos.InventarioMovimientos,
        PermisoCodigos.ReportesVer
    };

    /// <summary>Portal del paciente: solo inicio y consulta de sus citas.</summary>
    private static readonly IReadOnlyList<string> Paciente = new[]
    {
        PermisoCodigos.InicioVer,
        PermisoCodigos.CitasVer
    };

    /// <summary>
    /// Devuelve los permisos de respaldo del rol o <c>null</c> si el rol no esta
    /// reconocido. El seeder usa <c>null</c> para omitir roles personalizados en
    /// lugar de asignarles permisos por error.
    /// </summary>
    public static IReadOnlyList<string>? Obtener(string? nombreRol)
    {
        return Normalizar(nombreRol) switch
        {
            "administrador" or "admin" or "administrador del sistema"
                or "administrador sistema" => Administrador,

            "recepcionista" or "recepcion" or "recepcionista / asistente"
                or "recepcionista/asistente" or "asistente de recepcion"
                or "recepcionista y asistente" => Recepcionista,

            "odontologo" or "odontologa" or "doctor" or "doctora"
                or "odontologo / doctor" or "odontologo/doctor"
                or "odontologo doctor" => Odontologo,

            "asistente de odontologia" or "asistente odontologia"
                or "asistente" or "auxiliar de odontologia"
                or "auxiliar odontologia" => AsistenteOdontologia,

            "inventario" or "bodeguero" or "bodega"
                or "inventario / bodeguero" or "inventario/bodeguero"
                or "inventario y bodega" or "encargado de inventario"
                or "encargado de bodega" => InventarioBodeguero,

            "paciente" or "pacientes" => Paciente,

            _ => null
        };
    }

    /// <summary>
    /// Permisos de respaldo del rol. Si el rol no esta reconocido se devuelve
    /// solo el acceso a Inicio para no bloquear la aplicacion.
    /// </summary>
    public static IReadOnlyList<string> PorRol(string? nombreRol)
    {
        return Obtener(nombreRol) ?? Inicio;
    }

    private static string Normalizar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        var descompuesto = valor.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var texto = new StringBuilder(descompuesto.Length);

        foreach (var caracter in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter)
                != UnicodeCategory.NonSpacingMark)
            {
                texto.Append(caracter);
            }
        }

        return texto.ToString().Normalize(NormalizationForm.FormC);
    }
}
