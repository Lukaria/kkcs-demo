using System.Security.Claims;
using Kkcs.KeycloakDemo.Application.Contracts;
using Kkcs.KeycloakDemo.Domain;
using MapsterMapper;
using MediatR;

namespace Kkcs.KeycloakDemo.Application.Features.CurrentUser;

public sealed class GetCurrentUserQueryHandler(IMapper mapper)
    : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public Task<CurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (request.User.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("An authenticated user is required.");
        }

        var id = GetRequiredClaim(request.User, "sub");
        var username = request.User.FindFirst("preferred_username")?.Value
            ?? request.User.Identity.Name
            ?? GetRequiredClaim(request.User, "sub");
        var roles = request.User.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .Order()
            .ToArray();

        var user = new AuthenticatedUser(
            id,
            username,
            request.User.FindFirst("email")?.Value,
            request.User.FindFirst("given_name")?.Value,
            request.User.FindFirst("family_name")?.Value,
            roles);

        return Task.FromResult(mapper.Map<CurrentUserResponse>(user));
    }

    private static string GetRequiredClaim(ClaimsPrincipal user, string type) =>
        user.FindFirst(type)?.Value
        ?? throw new UnauthorizedAccessException($"The required '{type}' claim is missing.");
}
