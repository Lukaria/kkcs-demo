namespace Kkcs.KeycloakDemo.Application.Models;

public sealed record TokenResult(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    int RefreshExpiresIn,
    string TokenType);
