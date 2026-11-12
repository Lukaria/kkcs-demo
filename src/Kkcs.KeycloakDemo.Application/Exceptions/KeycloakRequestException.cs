using System.Net;

namespace Kkcs.KeycloakDemo.Application.Exceptions;

public sealed class KeycloakRequestException(HttpStatusCode statusCode, string message) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
