using Kkcs.KeycloakDemo.Application.Contracts;
using Kkcs.KeycloakDemo.Application.Features.Uptime;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kkcs.KeycloakDemo.Api.Controllers;

[ApiController]
public sealed class SystemController(ISender sender) : ControllerBase
{
    [HttpGet("uptime")]
    [AllowAnonymous]
    [ProducesResponseType<UptimeResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UptimeResponse>> Uptime(CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetUptimeQuery(), cancellationToken);
        return Ok(response);
    }
}
