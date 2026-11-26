namespace GestionOdontologica.Api.Security;

/// <summary>
/// Definicion de un permiso administrable. El codigo se deriva de MODULO +
/// ACCION y debe coincidir con una constante de <see cref="PermisoCodigos"/>.
/// </summary>
public sealed record PermisoDefinicion(
    string Codigo,
    string Modulo,
    string Accion,
    string Nombre,
    string Descripcion);

/// <summary>
/// Catalogo unico de permisos del sistema. Es la fuente de verdad que usa el
/// seeder para poblar la tabla PERMISOS y para validar que las politicas de
/// autorizacion tengan su permiso correspondiente.
///
/// No crea ni modifica estructuras: solo describe los datos que se insertan en
/// las tablas existentes.
/// </summary>
public static class PermisosCatalogo
{
    private static readonly IReadOnlyList<PermisoDefinicion> _todos = new[]
    {
        new PermisoDefinicion(
            PermisoCodigos.InicioVer, "Inicio", "VER", "Inicio - Ver",
            "Acceso al panel principal y al resumen general del sistema."),

        // ------------------------------------------------------------------
        // Seguridad: usuarios, roles y permisos
        // ------------------------------------------------------------------
        new PermisoDefinicion(
            PermisoCodigos.UsuariosVer, "Usuarios", "VER", "Usuarios - Ver",
            "Consultar el listado y el detalle de los usuarios."),
        new PermisoDefinicion(
            PermisoCodigos.UsuariosCrear, "Usuarios", "CREAR", "Usuarios - Crear",
            "Registrar nuevos usuarios en el sistema."),
        new PermisoDefinicion(
            PermisoCodigos.UsuariosEditar, "Usuarios", "EDITAR", "Usuarios - Editar",
            "Modificar los datos y el rol de los usuarios."),
        new PermisoDefinicion(
            PermisoCodigos.UsuariosEliminar, "Usuarios", "ELIMINAR", "Usuarios - Eliminar",
            "Dar de baja a los usuarios del sistema."),
        new PermisoDefinicion(
            PermisoCodigos.UsuariosReactivar, "Usuarios", "REACTIVAR", "Usuarios - Reactivar",
            "Reactivar usuarios que fueron dados de baja."),

        new PermisoDefinicion(
            PermisoCodigos.RolesVer, "Roles", "VER", "Roles - Ver",
            "Consultar los roles disponibles y sus descripciones."),
        new PermisoDefinicion(
            PermisoCodigos.RolesCrear, "Roles", "CREAR", "Roles - Crear",
            "Registrar nuevos roles en el sistema."),
        new PermisoDefinicion(
            PermisoCodigos.RolesEditar, "Roles", "EDITAR", "Roles - Editar",
            "Modificar el nombre y la descripcion de los roles."),
        new PermisoDefinicion(
            PermisoCodigos.RolesDesactivar, "Roles", "DESACTIVAR", "Roles - Desactivar",
            "Activar o desactivar roles existentes."),

        new PermisoDefinicion(
            PermisoCodigos.PermisosVer, "Permisos", "VER", "Permisos - Ver",
            "Consultar el catalogo de permisos y sus asignaciones."),
        new PermisoDefinicion(
            PermisoCodigos.PermisosCrear, "Permisos", "CREAR", "Permisos - Crear",
            "Registrar nuevos permisos en el catalogo."),
        new PermisoDefinicion(
            PermisoCodigos.PermisosEditar, "Permisos", "EDITAR", "Permisos - Editar",
            "Modificar los datos de un permiso existente."),
        new PermisoDefinicion(
            PermisoCodigos.PermisosEliminar, "Permisos", "ELIMINAR", "Permisos - Eliminar",
            "Eliminar permisos que no tengan roles asignados."),
        new PermisoDefinicion(
            PermisoCodigos.PermisosAsignar, "Permisos", "ASIGNAR", "Permisos - Asignar",
            "Asignar o quitar permisos a los roles."),

        // ------------------------------------------------------------------
        // Clinica: pacientes, odontologos, tratamientos y citas
        // ------------------------------------------------------------------
        new PermisoDefinicion(
            PermisoCodigos.PacientesVer, "Pacientes", "VER", "Pacientes - Ver",
            "Consultar el listado y el detalle de los pacientes."),
        new PermisoDefinicion(
            PermisoCodigos.PacientesCrear, "Pacientes", "CREAR", "Pacientes - Crear",
            "Registrar nuevos pacientes."),
        new PermisoDefinicion(
            PermisoCodigos.PacientesEditar, "Pacientes", "EDITAR", "Pacientes - Editar",
            "Modificar los datos de los pacientes."),
        new PermisoDefinicion(
            PermisoCodigos.PacientesDesactivar, "Pacientes", "DESACTIVAR", "Pacientes - Desactivar",
            "Dar de baja o reactivar pacientes."),
        new PermisoDefinicion(
            PermisoCodigos.PacientesExpediente, "Pacientes", "EXPEDIENTE", "Pacientes - Expediente",
            "Consultar y registrar antecedentes y expediente clinico."),

        new PermisoDefinicion(
            PermisoCodigos.OdontologosVer, "Odontólogos", "VER", "Odontólogos - Ver",
            "Consultar el listado y el detalle de los odontologos."),
        new PermisoDefinicion(
            PermisoCodigos.OdontologosCrear, "Odontólogos", "CREAR", "Odontólogos - Crear",
            "Registrar nuevos odontologos."),
        new PermisoDefinicion(
            PermisoCodigos.OdontologosEditar, "Odontólogos", "EDITAR", "Odontólogos - Editar",
            "Modificar los datos de los odontologos."),
        new PermisoDefinicion(
            PermisoCodigos.OdontologosDesactivar, "Odontólogos", "DESACTIVAR", "Odontólogos - Desactivar",
            "Dar de baja o reactivar odontologos."),
        new PermisoDefinicion(
            PermisoCodigos.OdontologosHorarios, "Odontólogos", "HORARIOS", "Odontólogos - Horarios",
            "Administrar los horarios de atencion de los odontologos."),
        new PermisoDefinicion(
            PermisoCodigos.OdontologosTratamientos, "Odontólogos", "TRATAMIENTOS", "Odontólogos - Tratamientos",
            "Asignar tratamientos que puede realizar cada odontologo."),

        new PermisoDefinicion(
            PermisoCodigos.TratamientosVer, "Tratamientos", "VER", "Tratamientos - Ver",
            "Consultar el catalogo de tratamientos."),
        new PermisoDefinicion(
            PermisoCodigos.TratamientosCrear, "Tratamientos", "CREAR", "Tratamientos - Crear",
            "Registrar nuevos tratamientos."),
        new PermisoDefinicion(
            PermisoCodigos.TratamientosEditar, "Tratamientos", "EDITAR", "Tratamientos - Editar",
            "Modificar precios y datos de los tratamientos."),
        new PermisoDefinicion(
            PermisoCodigos.TratamientosDesactivar, "Tratamientos", "DESACTIVAR", "Tratamientos - Desactivar",
            "Activar o desactivar tratamientos."),

        new PermisoDefinicion(
            PermisoCodigos.CitasVer, "Citas", "VER", "Citas - Ver",
            "Consultar la agenda y el detalle de las citas."),
        new PermisoDefinicion(
            PermisoCodigos.CitasCrear, "Citas", "CREAR", "Citas - Crear",
            "Agendar nuevas citas."),
        new PermisoDefinicion(
            PermisoCodigos.CitasEditar, "Citas", "EDITAR", "Citas - Editar",
            "Reprogramar o modificar citas existentes."),
        new PermisoDefinicion(
            PermisoCodigos.CitasCancelar, "Citas", "CANCELAR", "Citas - Cancelar",
            "Cancelar citas agendadas."),

        // ------------------------------------------------------------------
        // Inventario
        // ------------------------------------------------------------------
        new PermisoDefinicion(
            PermisoCodigos.InventarioVer, "Inventario", "VER", "Inventario - Ver",
            "Consultar productos, categorias y existencias del inventario."),
        new PermisoDefinicion(
            PermisoCodigos.InventarioCrear, "Inventario", "CREAR", "Inventario - Crear",
            "Registrar nuevos productos e insumos."),
        new PermisoDefinicion(
            PermisoCodigos.InventarioEditar, "Inventario", "EDITAR", "Inventario - Editar",
            "Modificar los datos y precios de los productos."),
        new PermisoDefinicion(
            PermisoCodigos.InventarioDesactivar, "Inventario", "DESACTIVAR", "Inventario - Desactivar",
            "Dar de baja o reactivar productos del inventario."),
        new PermisoDefinicion(
            PermisoCodigos.InventarioMovimientos, "Inventario", "MOVIMIENTOS", "Inventario - Movimientos",
            "Registrar entradas, salidas y ajustes de existencias."),

        // ------------------------------------------------------------------
        // Ventas, facturacion, pagos y cuentas por cobrar
        // ------------------------------------------------------------------
        new PermisoDefinicion(
            PermisoCodigos.PagosVer, "Pagos", "VER", "Pagos - Ver",
            "Consultar los pagos registrados y su estado."),
        new PermisoDefinicion(
            PermisoCodigos.PagosCrear, "Pagos", "CREAR", "Pagos - Crear",
            "Registrar pagos de pacientes."),
        new PermisoDefinicion(
            PermisoCodigos.PagosAnular, "Pagos", "ANULAR", "Pagos - Anular",
            "Anular o reversar pagos registrados."),

        new PermisoDefinicion(
            PermisoCodigos.FacturacionVer, "Facturación", "VER", "Facturación - Ver",
            "Consultar comprobantes y datos de facturacion."),
        new PermisoDefinicion(
            PermisoCodigos.FacturacionEmitir, "Facturación", "EMITIR", "Facturación - Emitir",
            "Emitir comprobantes y facturas."),

        new PermisoDefinicion(
            PermisoCodigos.CuentasPorCobrarVer, "Cuentas por cobrar", "VER", "Cuentas por cobrar - Ver",
            "Consultar saldos y cuentas pendientes de los pacientes."),
        new PermisoDefinicion(
            PermisoCodigos.CuentasPorCobrarGestionar, "Cuentas por cobrar", "GESTIONAR", "Cuentas por cobrar - Gestionar",
            "Registrar abonos y actualizar el estado de las cuentas por cobrar."),

        // ------------------------------------------------------------------
        // Promociones, auditoria y reportes
        // ------------------------------------------------------------------
        new PermisoDefinicion(
            PermisoCodigos.PromocionesVer, "Promociones", "VER", "Promociones - Ver",
            "Consultar promociones y paquetes de tratamientos."),
        new PermisoDefinicion(
            PermisoCodigos.PromocionesCrear, "Promociones", "CREAR", "Promociones - Crear",
            "Registrar nuevas promociones y paquetes."),
        new PermisoDefinicion(
            PermisoCodigos.PromocionesEditar, "Promociones", "EDITAR", "Promociones - Editar",
            "Modificar promociones y paquetes existentes."),
        new PermisoDefinicion(
            PermisoCodigos.PromocionesDesactivar, "Promociones", "DESACTIVAR", "Promociones - Desactivar",
            "Activar o desactivar promociones y paquetes."),

        new PermisoDefinicion(
            PermisoCodigos.BitacorasVer, "Bitácoras", "VER", "Bitácoras - Ver",
            "Consultar la bitacora de eventos y auditoria."),
        new PermisoDefinicion(
            PermisoCodigos.BitacorasExportar, "Bitácoras", "EXPORTAR", "Bitácoras - Exportar",
            "Exportar registros de auditoria."),

        new PermisoDefinicion(
            PermisoCodigos.ReportesVer, "Reportes", "VER", "Reportes - Ver",
            "Consultar reportes operativos y clinicos.")
    };

    private static readonly IReadOnlyDictionary<string, PermisoDefinicion> _porCodigo =
        _todos.ToDictionary(
            permiso => permiso.Codigo,
            StringComparer.OrdinalIgnoreCase);

    /// <summary>Todas las definiciones del catalogo, en orden de modulo.</summary>
    public static IReadOnlyList<PermisoDefinicion> Todos => _todos;

    /// <summary>Busca una definicion por codigo (por ejemplo "INVENTARIO_VER").</summary>
    public static PermisoDefinicion? PorCodigo(string codigo)
    {
        return _porCodigo.TryGetValue(codigo, out var permiso) ? permiso : null;
    }

    /// <summary>
    /// Verifica que el catalogo sea coherente con <see cref="PermisoCodigos"/>
    /// antes de sembrar datos. Falla rapido si hay codigos duplicados, nombres
    /// repetidos o codigos que no coinciden con MODULO + ACCION.
    /// </summary>
    public static void Validar()
    {
        var errores = new List<string>();
        var codigos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var nombres = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var permiso in _todos)
        {
            if (!codigos.Add(permiso.Codigo))
            {
                errores.Add($"Codigo duplicado: {permiso.Codigo}.");
            }

            if (!nombres.Add(permiso.Nombre))
            {
                errores.Add($"Nombre duplicado: {permiso.Nombre}.");
            }

            var esperado = PermisoCodigos.CrearCodigo(permiso.Modulo, permiso.Accion);

            if (!string.Equals(esperado, permiso.Codigo, StringComparison.OrdinalIgnoreCase))
            {
                errores.Add(
                    $"El codigo {permiso.Codigo} no coincide con MODULO + ACCION " +
                    $"({permiso.Modulo} + {permiso.Accion} => {esperado}).");
            }

            if (permiso.Nombre.Length > 100)
            {
                errores.Add($"El nombre {permiso.Codigo} supera los 100 caracteres.");
            }

            if (permiso.Modulo.Length > 50)
            {
                errores.Add($"El modulo {permiso.Codigo} supera los 50 caracteres.");
            }

            if (permiso.Accion.Length > 30)
            {
                errores.Add($"La accion {permiso.Codigo} supera los 30 caracteres.");
            }

            if (permiso.Descripcion.Length > 255)
            {
                errores.Add($"La descripcion {permiso.Codigo} supera los 255 caracteres.");
            }
        }

        foreach (var codigo in PermisoCodigos.Todos)
        {
            if (!codigos.Contains(codigo))
            {
                errores.Add($"El codigo {codigo} esta en PermisoCodigos pero falta en el catalogo.");
            }
        }

        foreach (var codigo in codigos)
        {
            if (!PermisoCodigos.Todos.Contains(codigo, StringComparer.OrdinalIgnoreCase))
            {
                errores.Add($"El codigo {codigo} esta en el catalogo pero falta en PermisoCodigos.Todos.");
            }
        }

        if (errores.Count > 0)
        {
            throw new InvalidOperationException(
                "Catalogo de permisos invalido: " + string.Join(" | ", errores));
        }
    }
}
