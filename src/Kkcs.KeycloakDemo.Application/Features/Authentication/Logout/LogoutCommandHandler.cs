using Kkcs.KeycloakDemo.Application.Abstractions;
using MediatR;

namespace Kkcs.KeycloakDemo.Application.Features.Authentication.Logout;

public sealed class LogoutCommandHandler(
    IKeycloakAuthenticationService authenticationService) : IRequestHandler<LogoutCommand>
{
    public Task Handle(LogoutCommand request, CancellationToken cancellationToken) =>
        authenticationService.LogoutAsync(request.RefreshToken, cancellationToken);
}
