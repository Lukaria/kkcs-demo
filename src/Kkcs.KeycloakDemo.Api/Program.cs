using Kkcs.KeycloakDemo.Api.Authentication;
using Kkcs.KeycloakDemo.Api.Middleware;
using Kkcs.KeycloakDemo.Application;
using Kkcs.KeycloakDemo.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddAuthentication("KeycloakJwt")
        .AddScheme<AuthenticationSchemeOptions, KeycloakJwtChallengeHandler>("KeycloakJwt", null);
    builder.Services.AddAuthorization();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Keycloak Authentication Demo API",
            Version = "v1",
            Description = "Custom JWT parsing and validation with Keycloak."
        });

        var bearerScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Access token without the 'Bearer' prefix.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        };
        options.AddSecurityDefinition("Bearer", bearerScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            [bearerScheme] = Array.Empty<string>()
        });
    });

    var app = builder.Build();

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMiddleware<KeycloakJwtAuthenticationMiddleware>();
    app.UseAuthorization();
    app.MapControllers();
    app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();

    Log.Information("Starting Keycloak authentication demo API");
    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "The application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
