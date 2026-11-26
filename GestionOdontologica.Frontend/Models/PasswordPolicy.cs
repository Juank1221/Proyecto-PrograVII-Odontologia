namespace GestionOdontologica.Frontend.Models;

/// <summary>
/// Réplica en el cliente de la política de contraseñas del backend. Permite
/// mostrar los errores antes de enviar la solicitud; la API vuelve a validar.
/// </summary>
public static class PasswordPolicy
{
    public const int LongitudMinima = 8;
    public const int LongitudMaxima = 72;

    public const string MensajeLongitud =
        "La contraseña debe tener al menos 8 caracteres.";

    public const string MensajeLongitudMaxima =
        "La contraseña no puede superar los 72 caracteres.";

    public const string MensajeLetrasConsecutivas =
        "La contraseña no puede tener letras iguales consecutivas (por ejemplo, \"aa\" o \"bb\").";

    public const string MensajeNumerosConsecutivos =
        "La contraseña no puede tener números iguales consecutivos (por ejemplo, \"11\" o \"22\").";

    public static IReadOnlyList<string> ObtenerErrores(string? password)
    {
        var errores = new List<string>(3);

        if (string.IsNullOrWhiteSpace(password))
        {
            errores.Add("La contraseña es obligatoria.");
            return errores;
        }

        if (password.Length < LongitudMinima)
        {
            errores.Add(MensajeLongitud);
        }
        else if (password.Length > LongitudMaxima)
        {
            errores.Add(MensajeLongitudMaxima);
        }

        if (TieneLetrasIgualesConsecutivas(password))
        {
            errores.Add(MensajeLetrasConsecutivas);
        }

        if (TieneNumerosIgualesConsecutivos(password))
        {
            errores.Add(MensajeNumerosConsecutivos);
        }

        return errores;
    }

    public static bool EsValida(string? password) => ObtenerErrores(password).Count == 0;

    private static bool TieneLetrasIgualesConsecutivas(string password)
    {
        for (var i = 1; i < password.Length; i++)
        {
            var actual = password[i];
            var anterior = password[i - 1];

            if (char.IsLetter(actual)
                && char.IsLetter(anterior)
                && char.ToLowerInvariant(actual) == char.ToLowerInvariant(anterior))
            {
                return true;
            }
        }

        return false;
    }

    private static bool TieneNumerosIgualesConsecutivos(string password)
    {
        for (var i = 1; i < password.Length; i++)
        {
            if (char.IsDigit(password[i]) && password[i] == password[i - 1])
            {
                return true;
            }
        }

        return false;
    }
}
