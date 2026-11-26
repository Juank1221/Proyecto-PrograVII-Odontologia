using Microsoft.AspNetCore.Authorization;

namespace GestionOdontologica.Api.Security;

/// <summary>Exige que el usuario tenga un permiso concreto.</summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string codigo)
    {
        Codigo = codigo;
    }

    public string Codigo { get; }
}
