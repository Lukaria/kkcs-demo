using Kkcs.KeycloakDemo.Application.Abstractions;
using Kkcs.KeycloakDemo.Infrastructure.Authentication;
using Kkcs.KeycloakDemo.Infrastructure.Keycloak;
using Kkcs.KeycloakDemo.Infrastructure.Uptime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kkcs.KeycloakDemo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<KeycloakOptions>()
            .Bind(configuration.GetSection(KeycloakOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _),
                "Keycloak BaseUrl must be an absolute URL.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Realm), "Keycloak Realm is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ClientId), "Keycloak ClientId is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Keycloak Audience is required.")
            .ValidateOnStart();

        services.AddHttpClient<IKeycloakAuthenticationService, KeycloakAuthenticationService>();
        services.AddSingleton<KeycloakJwtTokenValidator>();
        services.AddSingleton<IUptimeProvider, SystemUptimeProvider>();

        return services;
    }
}
