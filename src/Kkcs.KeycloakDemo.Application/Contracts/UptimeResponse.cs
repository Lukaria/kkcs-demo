namespace Kkcs.KeycloakDemo.Application.Contracts;

public sealed record UptimeResponse(DateTimeOffset StartedAtUtc, double UptimeSeconds);
