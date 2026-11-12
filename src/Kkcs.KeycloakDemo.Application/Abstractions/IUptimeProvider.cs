namespace Kkcs.KeycloakDemo.Application.Abstractions;

public interface IUptimeProvider
{
    DateTimeOffset StartedAtUtc { get; }

    TimeSpan Uptime { get; }
}
