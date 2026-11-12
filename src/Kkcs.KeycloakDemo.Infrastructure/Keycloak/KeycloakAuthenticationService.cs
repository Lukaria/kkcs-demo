using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kkcs.KeycloakDemo.Application.Abstractions;
using Kkcs.KeycloakDemo.Application.Exceptions;
using Kkcs.KeycloakDemo.Application.Models;
using Microsoft.Extensions.Options;

namespace Kkcs.KeycloakDemo.Infrastructure.Keycloak;

public sealed class KeycloakAuthenticationService(
    HttpClient httpClient,
    IOptions<KeycloakOptions> options) : IKeycloakAuthenticationService
{
    private readonly KeycloakOptions _options = options.Value;

    public async Task<TokenResult> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken)
    {
        var parameters = ClientCredentials();
        parameters["grant_type"] = "password";
        parameters["username"] = username;
        parameters["password"] = password;
        parameters["scope"] = "openid profile email";

        using var response = await httpClient.PostAsync(
            _options.TokenEndpoint,
            new FormUrlEncodedContent(parameters),
            cancellationToken);

        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateRequestException(response, cancellationToken);
        }

        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        var tokens = await JsonSerializer.DeserializeAsync<KeycloakTokenResponse>(
            content,
            cancellationToken: cancellationToken);

        if (tokens is null
            || string.IsNullOrWhiteSpace(tokens.AccessToken)
            || string.IsNullOrWhiteSpace(tokens.RefreshToken))
        {
            throw new KeycloakRequestException(
                HttpStatusCode.BadGateway,
                "Keycloak returned an incomplete token response.");
        }

        return new TokenResult(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.ExpiresIn,
            tokens.RefreshExpiresIn,
            tokens.TokenType ?? "Bearer");
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var parameters = ClientCredentials();
        parameters["refresh_token"] = refreshToken;

        using var response = await httpClient.PostAsync(
            _options.LogoutEndpoint,
            new FormUrlEncodedContent(parameters),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await CreateRequestException(response, cancellationToken);
        }
    }

    private Dictionary<string, string> ClientCredentials()
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId
        };

        if (!string.IsNullOrWhiteSpace(_options.ClientSecret))
        {
            parameters["client_secret"] = _options.ClientSecret;
        }

        return parameters;
    }

    private static async Task<KeycloakRequestException> CreateRequestException(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var description = await ReadErrorDescription(response, cancellationToken);
        var message = string.IsNullOrWhiteSpace(description)
            ? $"Keycloak rejected the request with status {(int)response.StatusCode}."
            : $"Keycloak rejected the request: {description}";

        return new KeycloakRequestException(response.StatusCode, message);
    }

    private static async Task<string?> ReadErrorDescription(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            var error = await JsonSerializer.DeserializeAsync<KeycloakErrorResponse>(
                content,
                cancellationToken: cancellationToken);
            return error?.ErrorDescription ?? error?.Error;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; init; }

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }

        [JsonPropertyName("refresh_expires_in")]
        public int RefreshExpiresIn { get; init; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; init; }
    }

    private sealed class KeycloakErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; init; }

        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; init; }
    }
}
