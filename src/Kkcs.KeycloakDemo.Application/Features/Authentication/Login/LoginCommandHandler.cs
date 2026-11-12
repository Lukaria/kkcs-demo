using Kkcs.KeycloakDemo.Application.Abstractions;
using Kkcs.KeycloakDemo.Application.Contracts;
using MapsterMapper;
using MediatR;

namespace Kkcs.KeycloakDemo.Application.Features.Authentication.Login;

public sealed class LoginCommandHandler(
    IKeycloakAuthenticationService authenticationService,
    IMapper mapper) : IRequestHandler<LoginCommand, TokenResponse>
{
    public async Task<TokenResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var tokens = await authenticationService.LoginAsync(
            request.Username,
            request.Password,
            cancellationToken);

        return mapper.Map<TokenResponse>(tokens);
    }
}
