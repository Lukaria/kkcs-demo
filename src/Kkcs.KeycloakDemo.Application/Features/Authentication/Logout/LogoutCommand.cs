using MediatR;

namespace Kkcs.KeycloakDemo.Application.Features.Authentication.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest;
