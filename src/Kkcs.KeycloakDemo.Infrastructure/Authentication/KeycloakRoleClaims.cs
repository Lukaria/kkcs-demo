using System.Security.Claims;
using System.Text.Json;

namespace Kkcs.KeycloakDemo.Infrastructure.Authentication;

public static class KeycloakRoleClaims
{
    public static void Add(ClaimsIdentity identity, string clientId)
    {
        using var realmDocument = ReadJson(identity.FindFirst("realm_access")?.Value);
        if (realmDocument is not null
            && realmDocument.RootElement.TryGetProperty("roles", out var realmRoles))
        {
            AddRoles(identity, realmRoles);
        }

        using var resourceDocument = ReadJson(identity.FindFirst("resource_access")?.Value);
        if (resourceDocument is not null
            && resourceDocument.RootElement.TryGetProperty(clientId, out var clientAccess)
            && clientAccess.TryGetProperty("roles", out var clientRoles))
        {
            AddRoles(identity, clientRoles);
        }
    }

    private static void AddRoles(ClaimsIdentity identity, JsonElement roles)
    {
        if (roles.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var role in roles.EnumerateArray())
        {
            if (role.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(role.GetString())
                && !identity.HasClaim(ClaimTypes.Role, role.GetString()!))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
            }
        }
    }

    private static JsonDocument? ReadJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return JsonDocument.Parse(value);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
