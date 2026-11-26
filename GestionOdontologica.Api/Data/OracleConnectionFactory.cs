using Oracle.ManagedDataAccess.Client;

namespace GestionOdontologica.Api.Data;

public sealed class OracleConnectionFactory : IOracleConnectionFactory
{
    private readonly string _connectionString;

    public OracleConnectionFactory(IConfiguration configuration)
    {
        // Se busca primero "Oracle" y como alternativa "DefaultConnection"
        _connectionString = configuration.GetConnectionString("Oracle")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "No se encontró la cadena de conexión 'Oracle' o 'DefaultConnection' en la configuración.");
    }

    public OracleConnection CreateConnection()
    {
        // ODP.NET usa AutoCommit = true por defecto. Se deja explícito para que
        // los INSERT/UPDATE del repositorio se confirmen al terminar cada
        // comando. Si se necesita atomicidad entre varias operaciones,
        // deberá usarse una OracleTransaction y establecer AutoCommit = false.
        return new OracleConnection(_connectionString)
        {
            AutoCommit = true
        };
    }
}
