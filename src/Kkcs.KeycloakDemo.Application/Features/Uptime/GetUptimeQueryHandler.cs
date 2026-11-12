using Kkcs.KeycloakDemo.Application.Abstractions;
using Kkcs.KeycloakDemo.Application.Contracts;
using MediatR;

namespace Kkcs.KeycloakDemo.Application.Features.Uptime;

public sealed class GetUptimeQueryHandler(IUptimeProvider uptimeProvider)
    : IRequestHandler<GetUptimeQuery, UptimeResponse>
{
    public Task<UptimeResponse> Handle(GetUptimeQuery request, CancellationToken cancellationToken) =>
        Task.FromResult(new UptimeResponse(
            uptimeProvider.StartedAtUtc,
            uptimeProvider.Uptime.TotalSeconds));
}
