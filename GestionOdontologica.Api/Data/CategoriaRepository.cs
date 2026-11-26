using System.Data;
using GestionOdontologica.Api.DTOs;
using Oracle.ManagedDataAccess.Client;

namespace GestionOdontologica.Api.Data;

public sealed class CategoriaRepository : ICategoriaRepository
{
    private const int CommandTimeoutSeconds = 60;
    private readonly IOracleConnectionFactory _connectionFactory;

    public CategoriaRepository(IOracleConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<CategoriaDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        using var command = new OracleCommand("sp_listar_categorias", connection)
        {
            CommandType = CommandType.StoredProcedure,
            BindByName = true,
            CommandTimeout = CommandTimeoutSeconds
        };
        var cursor = command.Parameters.Add("p_cursor", OracleDbType.RefCursor, ParameterDirection.Output);
        await command.ExecuteNonQueryAsync(cancellationToken);

        if (cursor.Value is not Oracle.ManagedDataAccess.Types.OracleRefCursor refCursor)
        {
            return Array.Empty<CategoriaDto>();
        }

        using var reader = refCursor.GetDataReader();
        var categorias = new List<CategoriaDto>();
        while (reader.Read())
        {
            categorias.Add(new CategoriaDto
            {
                IdCategoria = LeerEntero(reader, "ID_CATEGORIA"),
                Nombre = LeerTexto(reader, "NOMBRE"),
                Descripcion = LeerTexto(reader, "DESCRIPCION")
            });
        }

        return categorias;
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
}