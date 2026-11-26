namespace GestionOdontologica.Frontend.Models;

/// <summary>
/// Códigos de permisos compartidos con la API. Deben coincidir con
/// GestionOdontologica.Api.Security.PermisoCodigos y con la tabla PERMISOS.
/// </summary>
public static class PermisoCodigos
{
    public const string InicioVer = "INICIO_VER";

    public const string UsuariosVer = "USUARIOS_VER";
    public const string UsuariosCrear = "USUARIOS_CREAR";
    public const string UsuariosEditar = "USUARIOS_EDITAR";
    public const string UsuariosEliminar = "USUARIOS_ELIMINAR";
    public const string UsuariosReactivar = "USUARIOS_REACTIVAR";

    public const string RolesVer = "ROLES_VER";
    public const string RolesCrear = "ROLES_CREAR";
    public const string RolesEditar = "ROLES_EDITAR";
    public const string RolesDesactivar = "ROLES_DESACTIVAR";

    public const string PermisosVer = "PERMISOS_VER";
    public const string PermisosCrear = "PERMISOS_CREAR";
    public const string PermisosEditar = "PERMISOS_EDITAR";
    public const string PermisosEliminar = "PERMISOS_ELIMINAR";
    public const string PermisosAsignar = "PERMISOS_ASIGNAR";

    public const string PacientesVer = "PACIENTES_VER";
    public const string PacientesCrear = "PACIENTES_CREAR";
    public const string PacientesEditar = "PACIENTES_EDITAR";
    public const string PacientesDesactivar = "PACIENTES_DESACTIVAR";
    public const string PacientesExpediente = "PACIENTES_EXPEDIENTE";

    public const string OdontologosVer = "ODONTOLOGOS_VER";
    public const string OdontologosCrear = "ODONTOLOGOS_CREAR";
    public const string OdontologosEditar = "ODONTOLOGOS_EDITAR";
    public const string OdontologosDesactivar = "ODONTOLOGOS_DESACTIVAR";
    public const string OdontologosHorarios = "ODONTOLOGOS_HORARIOS";
    public const string OdontologosTratamientos = "ODONTOLOGOS_TRATAMIENTOS";

    public const string TratamientosVer = "TRATAMIENTOS_VER";
    public const string TratamientosCrear = "TRATAMIENTOS_CREAR";
    public const string TratamientosEditar = "TRATAMIENTOS_EDITAR";
    public const string TratamientosDesactivar = "TRATAMIENTOS_DESACTIVAR";

    public const string CitasVer = "CITAS_VER";
    public const string CitasCrear = "CITAS_CREAR";
    public const string CitasEditar = "CITAS_EDITAR";
    public const string CitasCancelar = "CITAS_CANCELAR";

    public const string InventarioVer = "INVENTARIO_VER";
    public const string InventarioCrear = "INVENTARIO_CREAR";
    public const string InventarioEditar = "INVENTARIO_EDITAR";
    public const string InventarioDesactivar = "INVENTARIO_DESACTIVAR";
    public const string InventarioMovimientos = "INVENTARIO_MOVIMIENTOS";

    public const string PagosVer = "PAGOS_VER";
    public const string PagosCrear = "PAGOS_CREAR";
    public const string PagosAnular = "PAGOS_ANULAR";

    public const string FacturacionVer = "FACTURACION_VER";
    public const string FacturacionEmitir = "FACTURACION_EMITIR";

    public const string CuentasPorCobrarVer = "CUENTAS_POR_COBRAR_VER";
    public const string CuentasPorCobrarGestionar = "CUENTAS_POR_COBRAR_GESTIONAR";

    public const string PromocionesVer = "PROMOCIONES_VER";
    public const string PromocionesCrear = "PROMOCIONES_CREAR";
    public const string PromocionesEditar = "PROMOCIONES_EDITAR";
    public const string PromocionesDesactivar = "PROMOCIONES_DESACTIVAR";

    public const string BitacorasVer = "BITACORAS_VER";
    public const string BitacorasExportar = "BITACORAS_EXPORTAR";

    public const string ReportesVer = "REPORTES_VER";

    public static readonly IReadOnlyList<string> Todos = new[]
    {
        InicioVer,
        UsuariosVer, UsuariosCrear, UsuariosEditar, UsuariosEliminar, UsuariosReactivar,
        RolesVer, RolesCrear, RolesEditar, RolesDesactivar,
        PermisosVer, PermisosCrear, PermisosEditar, PermisosEliminar, PermisosAsignar,
        PacientesVer, PacientesCrear, PacientesEditar, PacientesDesactivar, PacientesExpediente,
        OdontologosVer, OdontologosCrear, OdontologosEditar, OdontologosDesactivar,
        OdontologosHorarios, OdontologosTratamientos,
        TratamientosVer, TratamientosCrear, TratamientosEditar, TratamientosDesactivar,
        CitasVer, CitasCrear, CitasEditar, CitasCancelar,
        InventarioVer, InventarioCrear, InventarioEditar, InventarioDesactivar,
        InventarioMovimientos,
        PagosVer, PagosCrear, PagosAnular,
        FacturacionVer, FacturacionEmitir,
        CuentasPorCobrarVer, CuentasPorCobrarGestionar,
        PromocionesVer, PromocionesCrear, PromocionesEditar, PromocionesDesactivar,
        BitacorasVer, BitacorasExportar,
        ReportesVer
    };
}
