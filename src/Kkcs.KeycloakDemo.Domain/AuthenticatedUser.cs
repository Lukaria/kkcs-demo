namespace Kkcs.KeycloakDemo.Domain;

public sealed record AuthenticatedUser(
    string Id,
    string UserName,
    string? Email,
    string? FirstName,
    string? LastName,
    IReadOnlyCollection<string> Roles);
