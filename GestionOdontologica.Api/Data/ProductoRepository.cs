using System.Data;
using GestionOdontologica.Api.DTOs;
using Oracle.ManagedDataAccess.Client;

namespace GestionOdontologica.Api.Data;

public sealed class ProductoRepository : IProductoRepository
{
    private const int CommandTimeoutSeconds = 60;
    private readonly IOracleConnectionFactory _connectionFactory;

    public ProductoRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<ProductoDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var command = CrearProcedimiento("sp_listar_productos", connection);
        var cursor = AgregarCursorSalida(command);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return LeerProductos(cursor);
    }

    public async Task<ProductoDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var command = CrearProcedimiento("sp_obtener_producto", connection);
        AgregarEntrada(command, "p_id_producto", OracleDbType.Int32, null, id);
        var cursor = AgregarCursorSalida(command);
        await command.ExecuteNonQueryAsync(cancellationToken);
        var productos = LeerProductos(cursor);
        return productos.Count > 0 ? productos[0] : null;
    }

    public async Task<ProductoDto> InsertarAsync(CreacionProductoDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var command = CrearProcedimiento("sp_insertar_producto", connection);
        AgregarProductoEntradas(command, dto.IdCategoria, dto.Codigo, dto.Nombre, dto.Descripcion, dto.UnidadMedida, dto.StockMinimo, dto.PrecioCosto, dto.PrecioVenta, dto.RequiereCaducidad, true);
        var idSalida = command.Parameters.Add("p_id_producto", OracleDbType.Int32, ParameterDirection.Output);
        await command.ExecuteNonQueryAsync(cancellationToken);
        var producto = await ObtenerPorIdAsync(connection, idSalida.GetInt32Value(), cancellationToken);
        return producto ?? throw new InvalidOperationException("El producto se insertó, pero no fue posible recuperar el registro generado.");
    }

    public async Task<string> ObtenerSiguienteCodigoAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var command = CrearProcedimiento("sp_siguiente_codigo_producto", connection);

        var codigo = command.Parameters.Add("p_codigo", OracleDbType.Varchar2, ParameterDirection.Output);
        codigo.Size = 50;

        await command.ExecuteNonQueryAsync(cancellationToken);
        return codigo.Value is null or DBNull ? string.Empty : Convert.ToString(codigo.Value)?.Trim() ?? string.Empty;
    }

    public async Task<bool> ActualizarAsync(int id, ActualizacionProductoDto dto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dto);
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var actual = await ObtenerPorIdAsync(connection, id, cancellationToken);
        if (actual is null)
        {
            return false;
        }

        using var command = CrearProcedimiento("sp_actualizar_producto", connection);
        AgregarEntrada(command, "p_id_producto", OracleDbType.Int32, null, id);
        AgregarProductoEntradas(command, dto.IdCategoria, dto.Codigo, dto.Nombre, dto.Descripcion, dto.UnidadMedida, dto.StockMinimo, dto.PrecioCosto, dto.PrecioVenta, dto.RequiereCaducidad, dto.Activo ?? actual.Activo);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DesactivarAsync(int id, CancellationToken cancellationToken = default)
    {
        return await CambiarEstadoAsync("sp_desactivar_producto", id, cancellationToken);
    }

    public async Task<bool> ActivarAsync(int id, CancellationToken cancellationToken = default)
    {
        return await CambiarEstadoAsync("sp_activar_producto", id, cancellationToken);
    }

    private async Task<bool> CambiarEstadoAsync(string procedimiento, int id, CancellationToken cancellationToken)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var command = CrearProcedimiento(procedimiento, connection);
        AgregarEntrada(command, "p_id_producto", OracleDbType.Int32, null, id);
        await command.ExecuteNonQueryAsync(cancellationToken);
        return true;
    }

    private static async Task<ProductoDto?> ObtenerPorIdAsync(OracleConnection connection, int id, CancellationToken cancellationToken)
    {
        using var command = CrearProcedimiento("sp_obtener_producto", connection);
        AgregarEntrada(command, "p_id_producto", OracleDbType.Int32, null, id);
        var cursor = AgregarCursorSalida(command);
        await command.ExecuteNonQueryAsync(cancellationToken);
        var productos = LeerProductos(cursor);
        return productos.Count > 0 ? productos[0] : null;
    }

    private static void AgregarProductoEntradas(OracleCommand command, int idCategoria, string? codigo, string nombre, string? descripcion, string unidadMedida, int stockMinimo, decimal precioCosto, decimal precioVenta, bool requiereCaducidad, bool activo)
    {
        AgregarEntrada(command, "p_id_categoria", OracleDbType.Int32, null, idCategoria);
        AgregarEntrada(command, "p_codigo", OracleDbType.Varchar2, 50, string.IsNullOrWhiteSpace(codigo) ? null : codigo.Trim());
        AgregarEntrada(command, "p_nombre", OracleDbType.Varchar2, 150, nombre.Trim());
        AgregarEntrada(command, "p_descripcion", OracleDbType.Varchar2, 255, string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim());
        AgregarEntrada(command, "p_unidad_medida", OracleDbType.Varchar2, 30, unidadMedida.Trim());
        AgregarEntrada(command, "p_stock_minimo", OracleDbType.Int32, null, stockMinimo);
        AgregarEntrada(command, "p_precio_costo", OracleDbType.Decimal, null, precioCosto);
        AgregarEntrada(command, "p_precio_venta", OracleDbType.Decimal, null, precioVenta);
        AgregarEntrada(command, "p_requiere_caducidad", OracleDbType.Int32, null, requiereCaducidad ? 1 : 0);
        AgregarEntrada(command, "p_activo", OracleDbType.Int32, null, activo ? 1 : 0);
    }

    private static OracleCommand CrearProcedimiento(string nombre, OracleConnection connection) => new(nombre, connection)
    {
        CommandType = CommandType.StoredProcedure,
        BindByName = true,
        CommandTimeout = CommandTimeoutSeconds
    };

    private static OracleParameter AgregarCursorSalida(OracleCommand command) => command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);

    private static void AgregarEntrada(OracleCommand command, string nombre, OracleDbType tipo, int? tamanio, object? valor)
    {
        var parametro = tamanio is null ? command.Parameters.Add(nombre, tipo) : command.Parameters.Add(nombre, tipo, tamanio.Value);
        parametro.Value = valor ?? DBNull.Value;
    }

    private static IReadOnlyList<ProductoDto> LeerProductos(OracleParameter cursor)
    {
        if (cursor.Value is not Oracle.ManagedDataAccess.Types.OracleRefCursor refCursor)
        {
            return Array.Empty<ProductoDto>();
        }

        using var reader = refCursor.GetDataReader();
        var productos = new List<ProductoDto>();
        while (reader.Read())
        {
            var activoOrdinal = reader.GetOrdinal("ACTIVO");
            var activo = reader.IsDBNull(activoOrdinal) || Convert.ToInt32(reader.GetValue(activoOrdinal)) == 1;
            productos.Add(new ProductoDto
            {
                IdProducto = LeerEntero(reader, "ID_PRODUCTO"),
                IdCategoria = LeerEntero(reader, "ID_CATEGORIA"),
                NombreCategoria = LeerTexto(reader, "CATEGORIA"),
                Codigo = LeerTexto(reader, "CODIGO"),
                Nombre = LeerTexto(reader, "NOMBRE"),
                Descripcion = LeerTexto(reader, "DESCRIPCION"),
                UnidadMedida = LeerTexto(reader, "UNIDAD_MEDIDA"),
                StockMinimo = LeerEntero(reader, "STOCK_MINIMO"),
                PrecioCosto = LeerDecimal(reader, "PRECIO_COSTO"),
                PrecioVenta = LeerDecimal(reader, "PRECIO_VENTA"),
                RequiereCaducidad = LeerEntero(reader, "REQUIERE_CADUCIDAD") == 1,
                Activo = activo
            });
        }

        return productos;
    }

    private static string LeerTexto(OracleDataReader reader, string columna)
    {
        var ordinal = reader.GetOrdinal(columna);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }

    private static int LeerEntero(OracleDataReader reader, string columna)
    {
        var ordinal = reader.GetOrdinal(columna);
        return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));
    }

    private static decimal LeerDecimal(OracleDataReader reader, string columna)
    {
        var ordinal = reader.GetOrdinal(columna);
        return reader.IsDBNull(ordinal) ? 0m : Convert.ToDecimal(reader.GetValue(ordinal));
    }
}
