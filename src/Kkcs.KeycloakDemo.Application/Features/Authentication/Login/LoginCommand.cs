using Kkcs.KeycloakDemo.Application.Contracts;
using MediatR;

namespace Kkcs.KeycloakDemo.Application.Features.Authentication.Login;

public sealed record LoginCommand(string Username, string Password) : IRequest<TokenResponse>;
