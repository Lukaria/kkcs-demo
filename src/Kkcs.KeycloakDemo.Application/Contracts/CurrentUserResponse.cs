namespace Kkcs.KeycloakDemo.Application.Contracts;

public sealed record CurrentUserResponse(
    string Id,
    string UserName,
    string? Email,
    string? FirstName,
    string? LastName,
    IReadOnlyCollection<string> Roles);
