using System.Data;
using GestionOdontologica.Api.DTOs;
using GestionOdontologica.Api.Exceptions;
using GestionOdontologica.Api.Security;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace GestionOdontologica.Api.Data;

/// <summary>
/// Persistencia del modulo de permisos adaptada a la estructura real de
/// BD_ODONTOLOGIA:
///
///   PERMISOS       (ID_PERMISO, NOMBRE, MODULO, ACCION, DESCRIPCION)
///   ROL_PERMISOS   (ID_ROL, ID_PERMISO)
///   USUARIOS_ROLES (ID_USUARIO, ID_ROL)
///   ROLES          (ID_ROL, NOMBRE_ROL, DESCRIPCION, ACTIVO, FECHA_CREACION)
///   USUARIOS       (ID_USUARIO, ..., ROL, ACTIVO)
///
/// La tabla PERMISOS no tiene columna CODIGO, por lo que el codigo estable que
/// usan las politicas se deriva de MODULO + ACCION (por ejemplo
/// "Usuarios" + "VER" => "USUARIOS_VER"). Tampoco existen los procedimientos
/// sp_*permisos*, asi que todas las operaciones se resuelven con SQL directo
/// sin crear ni modificar objetos de la base de datos.
/// </summary>
public sealed class PermisoRepository : IPermisoRepository
{
    private const int CommandTimeoutSeconds = 60;

    // ORA-00942: table or view does not exist.
    private const int OracleTablaInexistente = 942;

    // ORA-00001: unique constraint violated.
    private const int OracleValorDuplicado = 1;

    private const string MensajeTablasFaltantes =
        "El modulo de permisos no esta disponible en la base de datos actual: " +
        "faltan las tablas PERMISOS y ROL_PERMISOS.";

    private readonly IOracleConnectionFactory _connectionFactory;

    public PermisoRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<PermisoDto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            using var command = CrearComando(
                connection,
                """
                SELECT p.ID_PERMISO,
                       p.NOMBRE,
                       p.MODULO,
                       p.ACCION,
                       p.DESCRIPCION,
                       (SELECT COUNT(*)
                          FROM ROL_PERMISOS rp
                         WHERE rp.ID_PERMISO = p.ID_PERMISO) AS ASIGNACIONES
                  FROM PERMISOS p
                 ORDER BY p.MODULO, p.NOMBRE
                """);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var permisos = new List<PermisoDto>();

            while (await reader.ReadAsync(cancellationToken))
            {
                permisos.Add(MapearPermiso(reader));
            }

            return permisos;
        }
        catch (OracleException ex) when (EsTablaInexistente(ex))
        {
            throw new PermisoNoDisponibleException(MensajeTablasFaltantes, ex);
        }
    }

    public async Task<PermisoDto?> ObtenerPorIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            return await ConsultarPorIdAsync(connection, id, cancellationToken);
        }
        catch (OracleException ex) when (EsTablaInexistente(ex))
        {
            throw new PermisoNoDisponibleException(MensajeTablasFaltantes, ex);
        }
    }

    public async Task<PermisoDto> InsertarAsync(
        CreacionPermisoDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            int idCreado;

            using (var command = CrearComando(
                connection,
                """
                INSERT INTO PERMISOS (NOMBRE, DESCRIPCION, MODULO, ACCION)
                VALUES (:p_nombre, :p_descripcion, :p_modulo, :p_accion)
                RETURNING ID_PERMISO INTO :p_id_permiso
                """))
            {
                AgregarEntrada(command, "p_nombre", OracleDbType.Varchar2, 100, dto.Nombre.Trim());
                AgregarEntrada(
                    command,
                    "p_descripcion",
                    OracleDbType.Varchar2,
                    255,
                    string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim());
                AgregarEntrada(command, "p_modulo", OracleDbType.Varchar2, 50, dto.Modulo.Trim());
                AgregarEntrada(command, "p_accion", OracleDbType.Varchar2, 30, dto.Accion.Trim());

                var idSalida = command.Parameters.Add(
                    "p_id_permiso",
                    OracleDbType.Int32,
                    ParameterDirection.Output);

                await command.ExecuteNonQueryAsync(cancellationToken);

                idCreado = ConvertirEnteroOracle(idSalida.Value);
            }

            var creado = await ConsultarPorIdAsync(
                connection,
                idCreado,
                cancellationToken);

            return creado ?? throw new InvalidOperationException(
                "El permiso se inserto, pero no fue posible recuperar el registro generado.");
        }
        catch (OracleException ex) when (ex.Number == OracleValorDuplicado)
        {
            throw new PermisoDuplicadoException(
                "Ya existe un permiso con ese nombre. Los nombres de permiso deben ser unicos.",
                ex);
        }
        catch (OracleException ex) when (EsTablaInexistente(ex))
        {
            throw new PermisoNoDisponibleException(MensajeTablasFaltantes, ex);
        }
    }

    public async Task<bool> ActualizarAsync(
        int id,
        ActualizacionPermisoDto dto,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        try
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
                """);

            AgregarEntrada(command, "p_id_permiso", OracleDbType.Int32, null, id);
            AgregarEntrada(command, "p_nombre", OracleDbType.Varchar2, 100, dto.Nombre.Trim());
            AgregarEntrada(
                command,
                "p_descripcion",
                OracleDbType.Varchar2,
                255,
                string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim());
            AgregarEntrada(command, "p_modulo", OracleDbType.Varchar2, 50, dto.Modulo.Trim());
            AgregarEntrada(command, "p_accion", OracleDbType.Varchar2, 30, dto.Accion.Trim());

            var filas = await command.ExecuteNonQueryAsync(cancellationToken);

            return filas > 0;
        }
        catch (OracleException ex) when (ex.Number == OracleValorDuplicado)
        {
            throw new PermisoDuplicadoException(
                "Ya existe otro permiso con ese nombre. Los nombres de permiso deben ser unicos.",
                ex);
        }
        catch (OracleException ex) when (EsTablaInexistente(ex))
        {
            throw new PermisoNoDisponibleException(MensajeTablasFaltantes, ex);
        }
    }

    public async Task<bool> EliminarAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            using (var asignaciones = CrearComando(
                connection,
                "SELECT COUNT(*) FROM ROL_PERMISOS WHERE ID_PERMISO = :p_id_permiso"))
            {
                AgregarEntrada(asignaciones, "p_id_permiso", OracleDbType.Int32, null, id);

                var total = Convert.ToInt32(
                    await asignaciones.ExecuteScalarAsync(cancellationToken));

                if (total > 0)
                {
                    throw new PermisoAsignadoException(
                        $"El permiso esta asignado a {total} rol(es). " +
                        "Quita las asignaciones antes de eliminarlo.");
                }
            }

            using var command = CrearComando(
                connection,
                "DELETE FROM PERMISOS WHERE ID_PERMISO = :p_id_permiso");

            AgregarEntrada(command, "p_id_permiso", OracleDbType.Int32, null, id);

            var filas = await command.ExecuteNonQueryAsync(cancellationToken);

            return filas > 0;
        }
        catch (OracleException ex) when (EsTablaInexistente(ex))
        {
            throw new PermisoNoDisponibleException(MensajeTablasFaltantes, ex);
        }
    }

    public async Task<IReadOnlyList<int>> ObtenerIdsPorRolAsync(
        int idRol,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        return await ObtenerIdsPorRolAsync(
            connection,
            null,
            idRol,
            cancellationToken);
    }

    public async Task<IReadOnlyList<string>> ObtenerCodigosPorNombreRolAsync(
        string nombreRol,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            var codigos = await ConsultarCodigosPorRolAsync(
                connection,
                nombreRol.Trim(),
                cancellationToken);

            if (codigos.Count > 0)
            {
                return codigos;
            }
        }
        catch (OracleException ex) when (EsTablaInexistente(ex))
        {
            // La base actual no tiene el modulo de permisos: se usan los
            // permisos predeterminados del rol para no bloquear el sistema.
        }

        return PermisosPredeterminados.PorRol(nombreRol);
    }

    public async Task<IReadOnlyList<string>> ObtenerCodigosPorUsuarioAsync(
        int idUsuario,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        try
        {
            var codigos = await ConsultarCodigosPorUsuarioAsync(
                connection,
                idUsuario,
                cancellationToken);

            if (codigos.Count > 0)
            {
                return codigos;
            }
        }
        catch (OracleException ex) when (EsTablaInexistente(ex))
        {
            // Sin tablas de permisos se aplica el conjunto predeterminado.
        }

        var rol = await ObtenerRolUsuarioAsync(connection, idUsuario, cancellationToken);

        return PermisosPredeterminados.PorRol(rol);
    }

    public async Task<bool> AsignarAsync(
        int idRol,
        int idPermiso,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        return await AsignarAsync(
            connection,
            null,
            idRol,
            idPermiso,
            cancellationToken);
    }

    public async Task<bool> QuitarAsync(
        int idRol,
        int idPermiso,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        return await QuitarAsync(
            connection,
            null,
            idRol,
            idPermiso,
            cancellationToken);
    }

    public async Task<bool> ReemplazarAsignacionesAsync(
        int idRol,
        IReadOnlyCollection<int> idPermisos,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(idPermisos);

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);

        var autoCommitOriginal = connection.AutoCommit;
        connection.AutoCommit = false;

        using var transaction = connection.BeginTransaction();

        try
        {
            var actuales = (await ObtenerIdsPorRolAsync(
                    connection,
                    transaction,
                    idRol,
                    cancellationToken))
                .ToHashSet();

            var deseados = idPermisos
                .Where(id => id > 0)
                .Distinct()
                .ToHashSet();

            var bajas = actuales.Except(deseados).ToList();
            var altas = deseados.Except(actuales).ToList();

            foreach (var idPermiso in bajas)
            {
                await QuitarAsync(
                    connection,
                    transaction,
                    idRol,
                    idPermiso,
                    cancellationToken);
            }

            foreach (var idPermiso in altas)
            {
                await AsignarAsync(
                    connection,
                    transaction,
                    idRol,
                    idPermiso,
                    cancellationToken);
            }

            transaction.Commit();

            return true;
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

    private static async Task<PermisoDto?> ConsultarPorIdAsync(
        OracleConnection connection,
        int id,
        CancellationToken cancellationToken)
    {
        using var command = CrearComando(
            connection,
            """
            SELECT p.ID_PERMISO,
                   p.NOMBRE,
                   p.MODULO,
                   p.ACCION,
                   p.DESCRIPCION,
                   (SELECT COUNT(*)
                      FROM ROL_PERMISOS rp
                     WHERE rp.ID_PERMISO = p.ID_PERMISO) AS ASIGNACIONES
              FROM PERMISOS p
             WHERE p.ID_PERMISO = :p_id_permiso
            """);

        AgregarEntrada(command, "p_id_permiso", OracleDbType.Int32, null, id);

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapearPermiso(reader);
    }

    private static async Task<IReadOnlyList<int>> ObtenerIdsPorRolAsync(
        OracleConnection connection,
        OracleTransaction? transaction,
        int idRol,
        CancellationToken cancellationToken)
    {
        try
        {
            using var command = CrearComando(
                connection,
                """
                SELECT ID_PERMISO
                  FROM ROL_PERMISOS
                 WHERE ID_ROL = :p_id_rol
                 ORDER BY ID_PERMISO
                """,
                transaction);

            AgregarEntrada(command, "p_id_rol", OracleDbType.Int32, null, idRol);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var ids = new List<int>();

            while (await reader.ReadAsync(cancellationToken))
            {
                ids.Add(LeerEntero(reader, "ID_PERMISO"));
            }

            return ids;
        }
        catch (OracleException ex) when (EsTablaInexistente(ex))
        {
            throw new PermisoNoDisponibleException(MensajeTablasFaltantes, ex);
        }
    }

    private static async Task<IReadOnlyList<string>> ConsultarCodigosPorRolAsync(
        OracleConnection connection,
        string nombreRol,
        CancellationToken cancellationToken)
    {
        using var command = CrearComando(
            connection,
            """
            SELECT p.MODULO, p.ACCION
              FROM ROLES r
              JOIN ROL_PERMISOS rp ON rp.ID_ROL = r.ID_ROL
              JOIN PERMISOS p ON p.ID_PERMISO = rp.ID_PERMISO
             WHERE UPPER(TRIM(r.NOMBRE_ROL)) = UPPER(TRIM(:p_nombre_rol))
               AND r.ACTIVO = 1
            """);

        AgregarEntrada(command, "p_nombre_rol", OracleDbType.Varchar2, 50, nombreRol);

        return await LeerCodigosAsync(command, cancellationToken);
    }

    private static async Task<IReadOnlyList<string>> ConsultarCodigosPorUsuarioAsync(
        OracleConnection connection,
        int idUsuario,
        CancellationToken cancellationToken)
    {
        using var command = CrearComando(
            connection,
            """
            SELECT p.MODULO, p.ACCION
              FROM USUARIOS u
              JOIN ROLES r
                ON UPPER(TRIM(r.NOMBRE_ROL)) = UPPER(TRIM(u.ROL))
              JOIN ROL_PERMISOS rp ON rp.ID_ROL = r.ID_ROL
              JOIN PERMISOS p ON p.ID_PERMISO = rp.ID_PERMISO
             WHERE u.ID_USUARIO = :p_id_usuario
               AND u.ACTIVO = 1
               AND r.ACTIVO = 1
            UNION
            SELECT p.MODULO, p.ACCION
              FROM USUARIOS_ROLES ur
              JOIN ROLES r ON r.ID_ROL = ur.ID_ROL
              JOIN ROL_PERMISOS rp ON rp.ID_ROL = r.ID_ROL
              JOIN PERMISOS p ON p.ID_PERMISO = rp.ID_PERMISO
             WHERE ur.ID_USUARIO = :p_id_usuario
               AND r.ACTIVO = 1
            """);

        AgregarEntrada(command, "p_id_usuario", OracleDbType.Int32, null, idUsuario);

        return await LeerCodigosAsync(command, cancellationToken);
    }

    private static async Task<IReadOnlyList<string>> LeerCodigosAsync(
        OracleCommand command,
        CancellationToken cancellationToken)
    {
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var codigos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (await reader.ReadAsync(cancellationToken))
        {
            var modulo = LeerTextoOpcional(reader, "MODULO");
            var accion = LeerTextoOpcional(reader, "ACCION");
            var codigo = PermisoCodigos.CrearCodigo(modulo, accion);

            if (!string.IsNullOrWhiteSpace(codigo))
            {
                codigos.Add(codigo);
            }
        }

        return codigos.OrderBy(codigo => codigo, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static async Task<string?> ObtenerRolUsuarioAsync(
        OracleConnection connection,
        int idUsuario,
        CancellationToken cancellationToken)
    {
        try
        {
            using var command = CrearComando(
                connection,
                "SELECT ROL FROM USUARIOS WHERE ID_USUARIO = :p_id_usuario");

            AgregarEntrada(command, "p_id_usuario", OracleDbType.Int32, null, idUsuario);

            var resultado = await command.ExecuteScalarAsync(cancellationToken);

            return resultado is null or DBNull ? null : Convert.ToString(resultado);
        }
        catch (OracleException ex) when (EsTablaInexistente(ex))
        {
            return null;
        }
    }

    private static async Task<bool> AsignarAsync(
        OracleConnection connection,
        OracleTransaction? transaction,
        int idRol,
        int idPermiso,
        CancellationToken cancellationToken)
    {
        try
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

            var filas = await command.ExecuteNonQueryAsync(cancellationToken);

            return filas > 0;
        }
        catch (OracleException ex) when (ex.Number == OracleValorDuplicado)
        {
            return false;
        }
        catch (OracleException ex) when (EsTablaInexistente(ex))
        {
            throw new PermisoNoDisponibleException(MensajeTablasFaltantes, ex);
        }
    }

    private static async Task<bool> QuitarAsync(
        OracleConnection connection,
        OracleTransaction? transaction,
        int idRol,
        int idPermiso,
        CancellationToken cancellationToken)
    {
        try
        {
            using var command = CrearComando(
                connection,
                """
                DELETE FROM ROL_PERMISOS
                 WHERE ID_ROL = :p_id_rol
                   AND ID_PERMISO = :p_id_permiso
                """,
                transaction);

            AgregarEntrada(command, "p_id_rol", OracleDbType.Int32, null, idRol);
            AgregarEntrada(command, "p_id_permiso", OracleDbType.Int32, null, idPermiso);

            var filas = await command.ExecuteNonQueryAsync(cancellationToken);

            return filas > 0;
        }
        catch (OracleException ex) when (EsTablaInexistente(ex))
        {
            throw new PermisoNoDisponibleException(MensajeTablasFaltantes, ex);
        }
    }

    private static OracleCommand CrearComando(
        OracleConnection connection,
        string sql,
        OracleTransaction? transaction = null)
    {
        return new OracleCommand(sql, connection)
        {
            CommandType = System.Data.CommandType.Text,
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

    private static PermisoDto MapearPermiso(OracleDataReader reader)
    {
        var nombre = LeerTexto(reader, "NOMBRE");
        var modulo = LeerTextoOpcional(reader, "MODULO") ?? string.Empty;
        var accion = LeerTextoOpcional(reader, "ACCION") ?? string.Empty;

        var codigo = PermisoCodigos.CrearCodigo(modulo, accion);

        if (string.IsNullOrWhiteSpace(codigo))
        {
            codigo = PermisoCodigos.CrearCodigo(nombre, null);
        }

        return new PermisoDto
        {
            IdPermiso = LeerEntero(reader, "ID_PERMISO"),
            Codigo = codigo,
            Nombre = nombre,
            Descripcion = LeerTextoOpcional(reader, "DESCRIPCION"),
            Modulo = modulo,
            Accion = accion,

            // La tabla PERMISOS real no tiene columna ESTADO ni FECHA_CREACION.
            // Se expone el permiso como vigente y sin fecha para conservar el
            // contrato del DTO que consume el frontend.
            Estado = true,
            FechaCreacion = DateTime.MinValue,
            Asignaciones = LeerEntero(reader, "ASIGNACIONES")
        };
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

        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetString(ordinal);
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

    private static bool EsTablaInexistente(OracleException ex)
        => ex.Number == OracleTablaInexistente;
}
