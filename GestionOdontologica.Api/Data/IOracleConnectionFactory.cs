using Oracle.ManagedDataAccess.Client;

namespace GestionOdontologica.Api.Data;

/// <summary>
/// Crea conexiones a Oracle. Se registra como singleton porque solo conserva
/// la cadena de conexión y no mantiene estado por solicitud.
/// </summary>
public interface IOracleConnectionFactory
{
    OracleConnection CreateConnection();
}
