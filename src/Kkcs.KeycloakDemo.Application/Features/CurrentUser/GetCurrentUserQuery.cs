using System.Security.Claims;
using Kkcs.KeycloakDemo.Application.Contracts;
using MediatR;

namespace Kkcs.KeycloakDemo.Application.Features.CurrentUser;

public sealed record GetCurrentUserQuery(ClaimsPrincipal User) : IRequest<CurrentUserResponse>;
