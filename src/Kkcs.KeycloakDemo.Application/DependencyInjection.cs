using FluentValidation;
using Kkcs.KeycloakDemo.Application.Behaviors;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Kkcs.KeycloakDemo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration => configuration
            .RegisterServicesFromAssembly(assembly)
            .AddOpenBehavior(typeof(ValidationBehavior<,>)));
        services.AddValidatorsFromAssembly(assembly);
        services.AddMapster();

        return services;
    }
}
