using Kkcs.KeycloakDemo.Application.Models;

namespace Kkcs.KeycloakDemo.Application.Abstractions;

public interface IKeycloakAuthenticationService
{
    Task<TokenResult> LoginAsync(string username, string password, CancellationToken cancellationToken);

    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken);
}
