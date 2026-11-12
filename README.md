# Keycloak Authentication Demo API

Демонстрационный ASP.NET Core 8 API с собственным middleware для проверки JWT access token, выданного Keycloak. Refresh token возвращается при входе и используется для завершения сеанса.

## Стек и структура

- `src/Kkcs.KeycloakDemo.Domain` — доменная модель пользователя.
- `src/Kkcs.KeycloakDemo.Application` — MediatR-команды и запросы, контракты, FluentValidation, Mapster.
- `src/Kkcs.KeycloakDemo.Infrastructure` — Keycloak OIDC-клиент, получение JWKS и проверка JWT, измерение uptime.
- `src/Kkcs.KeycloakDemo.Api` — middleware, эндпоинты, Swagger, Serilog и композиция приложения.

Используются MediatR, FluentValidation, Mapster, Serilog и Swashbuckle (Swagger). Проекты `Domain` и `Application` не зависят от ASP.NET Core или Keycloak SDK.

## Запуск

1. Запустите Keycloak и PostgreSQL:

   ```bash
   docker compose up -d
   ```

2. Один раз настройте realm, клиента и пользователя по инструкции [docs/keycloak-setup.md](docs/keycloak-setup.md).
3. Запустите API на хосте:

   ```bash
   dotnet run --project src/Kkcs.KeycloakDemo.Api --launch-profile http
   ```

4. Откройте Swagger: <http://localhost:5273/swagger>.

Настройки подключения находятся в `src/Kkcs.KeycloakDemo.Api/appsettings.json`. `RequireHttpsMetadata=false` допустимо только для локального HTTP Keycloak.

## Эндпоинты

| Метод | Путь | Аутентификация | Назначение |
|---|---|---|---|
| `POST` | `/login` | Не требуется | Получить access и refresh tokens по логину/паролю |
| `POST` | `/logout` | Не требуется | Завершить Keycloak-сеанс по refresh token |
| `GET` | `/me` | Bearer access token | Получить claims и роли текущего пользователя |
| `GET` | `/uptime` | Не требуется | Время запуска и uptime процесса |

Для `/me` в Swagger нажмите **Authorize** и вставьте только access token, без префикса `Bearer`.

Примеры запросов:

```bash
curl -X POST http://localhost:5273/login \
  -H "Content-Type: application/json" \
  -d '{"username":"demo-user","password":"Demo123!"}'

curl http://localhost:5273/me -H "Authorization: Bearer ACCESS_TOKEN"

curl -X POST http://localhost:5273/logout \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"REFRESH_TOKEN"}'
```

## Как работает собственный middleware

`KeycloakJwtAuthenticationMiddleware` читает `Authorization: Bearer ...`, а `KeycloakJwtTokenValidator`:

- загружает OpenID Connect metadata и JWKS Keycloak с кэшированием ключей;
- проверяет подпись RS256, `iss`, `aud`, `exp`/`nbf` и обязательное наличие подписи;
- допускает только access token с `typ=Bearer`;
- создаёт аутентифицированный `ClaimsPrincipal`;
- преобразует realm- и client-роли Keycloak в `ClaimTypes.Role`.

Стандартный `JwtBearerHandler` не используется. `UseAuthorization()` и `.RequireAuthorization()` работают с `HttpContext.User`, установленным собственным middleware.

## Ограничения демо

Вход по логину/паролю использует Keycloak Direct Access Grants (Resource Owner Password Credentials), чтобы API можно было проверить непосредственно из Swagger. Для production-приложений используйте Authorization Code Flow + PKCE на стороне клиента, HTTPS, секреты вне репозитория и более строгую политику токенов. Не передавайте refresh token в `Authorization`-заголовке.
