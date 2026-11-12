using Kkcs.KeycloakDemo.Application.Contracts;
using MediatR;

namespace Kkcs.KeycloakDemo.Application.Features.Uptime;

public sealed record GetUptimeQuery : IRequest<UptimeResponse>;
