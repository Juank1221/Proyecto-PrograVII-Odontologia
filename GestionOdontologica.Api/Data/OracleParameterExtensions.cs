using System.Globalization;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace GestionOdontologica.Api.Data;

/// <summary>
/// ODP.NET devuelve los parámetros NUMBER de salida como OracleDecimal, que
/// no implementa IConvertible. Este helper centraliza la conversión segura.
/// </summary>
internal static class OracleParameterExtensions
{
    public static int GetInt32Value(this OracleParameter parameter)
    {
        return parameter.Value switch
        {
            null or DBNull => 0,
            OracleDecimal valor => valor.ToInt32(),
            _ => Convert.ToInt32(parameter.Value, CultureInfo.InvariantCulture)
        };
    }
}
