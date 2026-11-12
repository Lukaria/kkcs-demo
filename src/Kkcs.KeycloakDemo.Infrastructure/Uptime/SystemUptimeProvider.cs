using System.Diagnostics;
using Kkcs.KeycloakDemo.Application.Abstractions;

namespace Kkcs.KeycloakDemo.Infrastructure.Uptime;

public sealed class SystemUptimeProvider : IUptimeProvider
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public DateTimeOffset StartedAtUtc { get; } = DateTimeOffset.UtcNow;

    public TimeSpan Uptime => _stopwatch.Elapsed;
}
