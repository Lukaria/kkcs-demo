using FluentValidation;

namespace Kkcs.KeycloakDemo.Application.Features.Authentication.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty();
    }
}
