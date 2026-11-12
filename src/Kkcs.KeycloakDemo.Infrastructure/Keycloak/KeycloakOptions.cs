namespace Kkcs.KeycloakDemo.Infrastructure.Keycloak;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    public required string BaseUrl { get; init; }

    public required string Realm { get; init; }

    public required string ClientId { get; init; }

    public string? ClientSecret { get; init; }

    public required string Audience { get; init; }

    public bool RequireHttpsMetadata { get; init; } = true;

    public string Issuer => $"{BaseUrl.TrimEnd('/')}/realms/{Realm}";

    public string MetadataAddress => $"{Issuer}/.well-known/openid-configuration";

    public string TokenEndpoint => $"{Issuer}/protocol/openid-connect/token";

    public string LogoutEndpoint => $"{Issuer}/protocol/openid-connect/logout";
}
