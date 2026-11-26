using System.Data;
using GestionOdontologica.Api.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace GestionOdontologica.Api.Security;

/// <summary>
/// Puebla el catalogo de permisos y las asignaciones por rol al arrancar la
/// API. Solo ejecuta DML sobre las tablas existentes:
///
///   PERMISOS       (ID_PERMISO, NOMBRE, MODULO, ACCION, DESCRIPCION)
///   ROL_PERMISOS   (ID_ROL, ID_PERMISO)
///   ROLES          (ID_ROL, NOMBRE_ROL, ACTIVO)
///
/// El seed es idempotente:
///   - Actualiza el permiso si ya existe (por codigo MODULO + ACCION o por
///     NOMBRE) y lo inserta solo cuando no existe.
///   - Agrega asignaciones con INSERT ... WHERE NOT EXISTS y nunca elimina
///     asignaciones existentes, de modo que las personalizaciones del
///     administrador se conservan.
///   - Si la base actual no tiene las tablas del modulo (por ejemplo QA), se
///     omite con una advertencia y no bloquea el arranque.
///
/// Se controla con "Permisos:Seed". En appsettings.json (PROD) esta activo y
/// en appsettings.Development.json (QA) esta desactivado.
/// </summary>
public sealed class PermisoSeederHostedService : BackgroundService
{
    private const int CommandTimeoutSeconds = 60;

    private readonly IOracleConnectionFactory _connectionFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PermisoSeederHostedService> _logger;

    public PermisoSeederHostedService(
        IOracleConnectionFactory connectionFactory,
        IConfiguration configuration,
        ILogger<PermisoSeederHostedService> logger)
    {
        _connectionFactory = connectionFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_configuration.GetValue<bool>("Permisos:Seed"))
        {
            _logger.LogInformation(
                "Seed de permisos deshabilitado (Permisos:Seed=false).");
            return;
        }

        try
        {
            await SembrarAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Apagado normal de la aplicacion.
        }
        catch (Exception ex)
        {
            // Un fallo del seed no debe impedir que la API arranque.
            _logger.LogError(
                ex,
                "No se pudo completar el seed de permisos. La API continuara " +
                "funcionando con los permisos ya existentes.");
        }
    }

    private async Task SembrarAsync(CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var tablas = await ObtenerTablasAsync(connection, cancellationToken);

        if (!tablas.Contains("PERMISOS") || !tablas.Contains("ROL_PERMISOS"))
        {
            _logger.LogWarning(
                "Seed de permisos omitido: la base de datos actual no tiene las " +
                "tablas PERMISOS y ROL_PERMISOS. No se creo ningun objeto.");
            return;
        }

        var autoCommitOriginal = connection.AutoCommit;
        connection.AutoCommit = false;

        using var transaction = connection.BeginTransaction();

        try
        {
            var idsPorCodigo = await SincronizarCatalogoAsync(
                connection,
                transaction,
                cancellationToken);

            if (!tablas.Contains("ROLES"))
            {
                _logger.LogWarning(
                    "Seed de permisos: no existe la tabla ROLES; se omite la " +
                    "asignacion por rol.");
            }
            else if (_configuration.GetValue("Permisos:AsignarPorRol", true))
            {
                await AsignarPermisosARolesAsync(
                    connection,
                    transaction,
                    idsPorCodigo,
                    cancellationToken);
            }
            else
            {
                _logger.LogInformation(
                    "Seed de permisos: asignacion por rol deshabilitada " +
                    "(Permisos:AsignarPorRol=false).");
            }

            transaction.Commit();
        }
        catch
        {
            try
            {
                transaction.Rollback();
            }
            catch (OracleException)
            {
                // Se conserva la excepcion original.
            }

            throw;
        }
        finally
        {
            connection.AutoCommit = autoCommitOriginal;
        }
    }

    private static async Task<HashSet<string>> ObtenerTablasAsync(
        OracleConnection connection,
        CancellationToken cancellationToken)
    {
        using var command = CrearComando(
            connection,
            "SELECT TABLE_NAME FROM USER_TABLES");

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var tablas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (await reader.ReadAsync(cancellationToken))
        {
            tablas.Add(reader.GetString(0));
        }

        return tablas;
    }

    /// <summary>
    /// Sincroniza el catalogo C# con la tabla PERMISOS y devuelve el ID real de
    /// cada codigo para poder asignarlo a los roles.
    /// </summary>
    private async Task<Dictionary<string, int>> SincronizarCatalogoAsync(
        OracleConnection connection,
        OracleTransaction transaction,
        CancellationToken cancellationToken)
    {
        var existentes = await LeerPermisosAsync(
            connection,
            transaction,
            cancellationToken);

        var porCodigo = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var porNombre = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var permiso in existentes)
        {
            if (!string.IsNullOrWhiteSpace(permiso.Codigo))
            {
                // Si la base tuviera codigos duplicados se conserva el primero.
                porCodigo.TryAdd(permiso.Codigo, permiso.IdPermiso);
            }

            porNombre.TryAdd(NormalizarNombre(permiso.Nombre), permiso.IdPermiso);
        }

        var insertados = 0;
        var actualizados = 0;

        foreach (var definicion in PermisosCatalogo.Todos)
        {
            var id = BuscarExistente(
                definicion,
                porCodigo,
                porNombre);

            if (id is null)
            {
                id = await InsertarPermisoAsync(
                    connection,
                    transaction,
                    definicion,
                    cancellationToken);

                insertados++;
            }
            else
            {
                await ActualizarPermisoAsync(
                    connection,
                    transaction,
                    id.Value,
                    definicion,
                    cancellationToken);

                actualizados++;
            }

            porCodigo[definicion.Codigo] = id.Value;
            porNombre[NormalizarNombre(definicion.Nombre)] = id.Value;
        }

        _logger.LogInformation(
            "Catalogo de permisos sincronizado: {Insertados} insertados, " +
            "{Actualizados} actualizados, {Total} en el catalogo.",
            insertados,
            actualizados,
            PermisosCatalogo.Todos.Count);

        return porCodigo;
    }

    private static int? BuscarExistente(
        PermisoDefinicion definicion,
        IReadOnlyDictionary<string, int> porCodigo,
        IReadOnlyDictionary<string, int> porNombre)
    {
        var nombre = NormalizarNombre(definicion.Nombre);

        if (porCodigo.TryGetValue(definicion.Codigo, out var idCodigo))
        {
            // Si otra fila ya usa el mismo nombre, se prefiere esa fila para no
            // violar la restriccion unica UQ_PERMISOS(NOMBRE).
            if (porNombre.TryGetValue(nombre, out var idNombre) && idNombre != idCodigo)
            {
                return idNombre;
            }

            return idCodigo;
        }

        return porNombre.TryGetValue(nombre, out var id) ? id : null;
    }

    private static async Task<List<PermisoExistente>> LeerPermisosAsync(
        OracleConnection connection,
        OracleTransaction transaction,
        CancellationToken cancellationToken)
    {
        using var command = CrearComando(
            connection,
            """
            SELECT ID_PERMISO, NOMBRE, MODULO, ACCION
              FROM PERMISOS
             ORDER BY ID_PERMISO
            """,
            transaction);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var permisos = new List<PermisoExistente>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var modulo = LeerTextoOpcional(reader, "MODULO");
            var accion = LeerTextoOpcional(reader, "ACCION");

            permisos.Add(new PermisoExistente(
                LeerEntero(reader, "ID_PERMISO"),
                LeerTexto(reader, "NOMBRE"),
                PermisoCodigos.CrearCodigo(modulo, accion)));
        }

        return permisos;
    }

    private static async Task<int> InsertarPermisoAsync(
        OracleConnection connection,
        OracleTransaction transaction,
        PermisoDefinicion definicion,
        CancellationToken cancellationToken)
    {
        using var command = CrearComando(
            connection,
            """
            INSERT INTO PERMISOS (NOMBRE, DESCRIPCION, MODULO, ACCION)
            VALUES (:p_nombre, :p_descripcion, :p_modulo, :p_accion)
            RETURNING ID_PERMISO INTO :p_id_permiso
            """,
            transaction);

        AgregarEntrada(command, "p_nombre", OracleDbType.Varchar2, 100, definicion.Nombre);
        AgregarEntrada(command, "p_descripcion", OracleDbType.Varchar2, 255, definicion.Descripcion);
        AgregarEntrada(command, "p_modulo", OracleDbType.Varchar2, 50, definicion.Modulo);
        AgregarEntrada(command, "p_accion", OracleDbType.Varchar2, 30, definicion.Accion);

        var idSalida = command.Parameters.Add(
            "p_id_permiso",
            OracleDbType.Int32,
            ParameterDirection.Output);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return ConvertirEnteroOracle(idSalida.Value);
    }

    private static async Task ActualizarPermisoAsync(
        OracleConnection connection,
        OracleTransaction transaction,
        int idPermiso,
        PermisoDefinicion definicion,
        CancellationToken cancellationToken)
    {
        using var command = CrearComando(
            connection,
            """
            UPDATE PERMISOS
               SET NOMBRE      = :p_nombre,
                   DESCRIPCION = :p_descripcion,
                   MODULO      = :p_modulo,
                   ACCION      = :p_accion
             WHERE ID_PERMISO  = :p_id_permiso
            """,
            transaction);

        AgregarEntrada(command, "p_id_permiso", OracleDbType.Int32, null, idPermiso);
        AgregarEntrada(command, "p_nombre", OracleDbType.Varchar2, 100, definicion.Nombre);
        AgregarEntrada(command, "p_descripcion", OracleDbType.Varchar2, 255, definicion.Descripcion);
        AgregarEntrada(command, "p_modulo", OracleDbType.Varchar2, 50, definicion.Modulo);
        AgregarEntrada(command, "p_accion", OracleDbType.Varchar2, 30, definicion.Accion);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task AsignarPermisosARolesAsync(
        OracleConnection connection,
        OracleTransaction transaction,
        IReadOnlyDictionary<string, int> idsPorCodigo,
        CancellationToken cancellationToken)
    {
        var roles = await LeerRolesActivosAsync(
            connection,
            transaction,
            cancellationToken);

        var asignados = 0;
        var omitidos = 0;

        foreach (var rol in roles)
        {
            var permisos = PermisosPredeterminados.Obtener(rol.Nombre);

            if (permisos is null)
            {
                omitidos++;
                _logger.LogWarning(
                    "Seed de permisos: el rol {Rol} (ID {IdRol}) no esta " +
                    "reconocido en la matriz; se omite sin asignar permisos.",
                    rol.Nombre,
                    rol.IdRol);
                continue;
            }

            foreach (var codigo in permisos)
            {
                if (!idsPorCodigo.TryGetValue(codigo, out var idPermiso))
                {
                    _logger.LogWarning(
                        "Seed de permisos: el codigo {Codigo} del rol {Rol} no " +
                        "existe en el catalogo; se omite.",
                        codigo,
                        rol.Nombre);
                    continue;
                }

                asignados += await AsignarAsync(
                    connection,
                    transaction,
                    rol.IdRol,
                    idPermiso,
                    cancellationToken);
            }
        }

        _logger.LogInformation(
            "Asignaciones de permisos sincronizadas: {Asignados} nuevas, " +
            "{Roles} roles activos, {Omitidos} roles sin matriz.",
            asignados,
            roles.Count,
            omitidos);
    }

    private static async Task<List<RolActivo>> LeerRolesActivosAsync(
        OracleConnection connection,
        OracleTransaction transaction,
        CancellationToken cancellationToken)
    {
        using var command = CrearComando(
            connection,
            """
            SELECT ID_ROL, NOMBRE_ROL
              FROM ROLES
             WHERE ACTIVO = 1
             ORDER BY ID_ROL
            """,
            transaction);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var roles = new List<RolActivo>();

        while (await reader.ReadAsync(cancellationToken))
        {
            roles.Add(new RolActivo(
                LeerEntero(reader, "ID_ROL"),
                LeerTexto(reader, "NOMBRE_ROL")));
        }

        return roles;
    }

    private static async Task<int> AsignarAsync(
        OracleConnection connection,
        OracleTransaction transaction,
        int idRol,
        int idPermiso,
        CancellationToken cancellationToken)
    {
        using var command = CrearComando(
            connection,
            """
            INSERT INTO ROL_PERMISOS (ID_ROL, ID_PERMISO)
            SELECT :p_id_rol, :p_id_permiso
              FROM DUAL
             WHERE NOT EXISTS (
                   SELECT 1
                     FROM ROL_PERMISOS
                    WHERE ID_ROL = :p_id_rol
                      AND ID_PERMISO = :p_id_permiso)
            """,
            transaction);

        AgregarEntrada(command, "p_id_rol", OracleDbType.Int32, null, idRol);
        AgregarEntrada(command, "p_id_permiso", OracleDbType.Int32, null, idPermiso);

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static OracleCommand CrearComando(
        OracleConnection connection,
        string sql,
        OracleTransaction? transaction = null)
    {
        return new OracleCommand(sql, connection)
        {
            CommandType = CommandType.Text,
            BindByName = true,
            CommandTimeout = CommandTimeoutSeconds,
            Transaction = transaction
        };
    }

    private static void AgregarEntrada(
        OracleCommand command,
        string nombre,
        OracleDbType tipo,
        int? tamanio,
        object? valor)
    {
        var parametro = tamanio is null
            ? command.Parameters.Add(nombre, tipo)
            : command.Parameters.Add(nombre, tipo, tamanio.Value);

        parametro.Value = valor ?? DBNull.Value;
    }

    private static string NormalizarNombre(string? nombre)
    {
        return string.IsNullOrWhiteSpace(nombre)
            ? string.Empty
            : nombre.Trim().ToUpperInvariant();
    }

    private static string LeerTexto(OracleDataReader reader, string columna)
    {
        var ordinal = reader.GetOrdinal(columna);

        return reader.IsDBNull(ordinal)
            ? string.Empty
            : reader.GetString(ordinal);
    }

    private static string? LeerTextoOpcional(OracleDataReader reader, string columna)
    {
        var ordinal = reader.GetOrdinal(columna);

        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static int LeerEntero(OracleDataReader reader, string columna)
    {
        var ordinal = reader.GetOrdinal(columna);

        return reader.IsDBNull(ordinal)
            ? 0
            : Convert.ToInt32(reader.GetValue(ordinal));
    }

    private static int ConvertirEnteroOracle(object? valor)
    {
        return valor switch
        {
            null or DBNull => 0,
            OracleDecimal numero => numero.ToInt32(),
            _ => Convert.ToInt32(valor)
        };
    }

    private sealed record PermisoExistente(int IdPermiso, string Nombre, string Codigo);

    private sealed record RolActivo(int IdRol, string Nombre);
}
