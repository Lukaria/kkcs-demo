using Kkcs.KeycloakDemo.Application.Contracts;
using Kkcs.KeycloakDemo.Application.Features.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kkcs.KeycloakDemo.Api.Controllers;

[ApiController]
public sealed class AuthenticationController(ISender sender) : ControllerBase
{
    [HttpGet("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult Login() =>
        Challenge(
            new AuthenticationProperties { RedirectUri = "/me" },
            OpenIdConnectDefaults.AuthenticationScheme);

    [HttpGet("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult Logout() =>
        SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<CurrentUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetCurrentUserQuery(User), cancellationToken);
        return Ok(response);
    }
}
