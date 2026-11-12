using System.Security.Claims;
using System.Text.Json;
using Kkcs.KeycloakDemo.Infrastructure.Keycloak;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Kkcs.KeycloakDemo.Infrastructure.Authentication;

public sealed class KeycloakJwtTokenValidator
{
    private readonly KeycloakOptions _options;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    private readonly JsonWebTokenHandler _tokenHandler = new() { MapInboundClaims = false };

    public KeycloakJwtTokenValidator(IOptions<KeycloakOptions> options)
    {
        _options = options.Value;
        var documentRetriever = new HttpDocumentRetriever
        {
            RequireHttps = _options.RequireHttpsMetadata
        };

        _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            _options.MetadataAddress,
            new OpenIdConnectConfigurationRetriever(),
            documentRetriever)
        {
            AutomaticRefreshInterval = TimeSpan.FromHours(1),
            RefreshInterval = TimeSpan.FromMinutes(5)
        };
    }

    public async Task<ClaimsPrincipal> ValidateAsync(string token, CancellationToken cancellationToken)
    {
        var configuration = await _configurationManager.GetConfigurationAsync(cancellationToken);
        var validationParameters = new TokenValidationParameters
        {
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = configuration.SigningKeys,
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = true,
            ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };

        var result = await _tokenHandler.ValidateTokenAsync(token, validationParameters);
        if (!result.IsValid)
        {
            throw new SecurityTokenException("JWT validation failed.", result.Exception);
        }

        if (result.SecurityToken is not JsonWebToken jwt
            || !jwt.TryGetPayloadValue<string>("typ", out var tokenType)
            || !string.Equals(tokenType, "Bearer", StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityTokenException("The token is not a Keycloak access token.");
        }

        var identity = new ClaimsIdentity(
            result.ClaimsIdentity.Claims,
            "KeycloakJwt",
            "preferred_username",
            ClaimTypes.Role);

        AddKeycloakRoles(identity);
        return new ClaimsPrincipal(identity);
    }

    private void AddKeycloakRoles(ClaimsIdentity identity)
    {
        using var realmDocument = ReadJson(identity.FindFirst("realm_access")?.Value);
        if (realmDocument is not null
            && realmDocument.RootElement.TryGetProperty("roles", out var realmRoles))
        {
            AddRoles(identity, realmRoles);
        }

        using var resourceDocument = ReadJson(identity.FindFirst("resource_access")?.Value);
        if (resourceDocument is not null
            && resourceDocument.RootElement.TryGetProperty(_options.ClientId, out var clientAccess)
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
            if (role.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(role.GetString()))
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
