using FluentValidation;

namespace Kkcs.KeycloakDemo.Application.Features.Authentication.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Username)
            .NotEmpty()
            .MaximumLength(256);
        RuleFor(command => command.Password)
            .NotEmpty()
            .MaximumLength(1024);
    }
}
