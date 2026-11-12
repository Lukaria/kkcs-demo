# Keycloak Confidential Client Demo API

Ветка `confidential` демонстрирует вход через **Authorization Code Flow + PKCE** с confidential-клиентом Keycloak. Пароль вводится только на странице Keycloak: ASP.NET-приложение не получает учётные данные пользователя. Access, refresh и ID tokens сохраняются в зашифрованной authentication session и не возвращаются браузеру как JSON.

Дополнительно сохранён собственный middleware для проверки Bearer JWT при вызовах API из других клиентов.

## Стек и структура

- `src/Kkcs.KeycloakDemo.Domain` — доменная модель пользователя.
- `src/Kkcs.KeycloakDemo.Application` — MediatR-запросы, контракты, FluentValidation и Mapster.
- `src/Kkcs.KeycloakDemo.Infrastructure` — получение JWKS, проверка Bearer JWT, Keycloak claims и uptime.
- `src/Kkcs.KeycloakDemo.Api` — OIDC/cookie-аутентификация, контроллеры, Swagger и Serilog.

## Настройка и запуск

1. Запустите Keycloak и PostgreSQL:

   ```bash
   docker compose up -d
   ```

2. Настройте realm, confidential-клиент и пользователя по инструкции [docs/keycloak-setup.md](docs/keycloak-setup.md).
3. Сохраните секрет клиента локально:

   ```bash
   dotnet user-secrets set "Keycloak:ClientSecret" "ВАШ_СЕКРЕТ" --project src/Kkcs.KeycloakDemo.Api
   ```

4. Запустите API:

   ```bash
   dotnet run --project src/Kkcs.KeycloakDemo.Api --launch-profile http
   ```

5. Откройте <http://localhost:5273/login> в браузере. После входа Keycloak вернёт на `/me`.

Swagger доступен по адресу <http://localhost:5273/swagger>. Интерактивный OIDC-вход удобнее проверять непосредственно в браузере, а не через кнопку Swagger **Try it out**.

## Эндпоинты

| Метод | Путь | Аутентификация | Назначение |
|---|---|---|---|
| `GET` | `/login` | Не требуется | Перенаправить на Keycloak Authorization Code Flow + PKCE |
| `GET/POST` | `/signin-oidc` | Обрабатывается OIDC middleware | Принять authorization code и создать cookie-сессию |
| `GET` | `/logout` | Cookie-сессия | Завершить локальную и Keycloak-сессии |
| `GET` | `/me` | Cookie или Bearer JWT | Получить claims и роли текущего пользователя |
| `GET` | `/uptime` | Не требуется | Время запуска и uptime процесса |

Без cookie `/me` вызывает OIDC challenge и перенаправляет на Keycloak. При переданном `Authorization: Bearer ...` собственный middleware вместо cookie выполняет JWT-проверку; malformed или недействительный токен получает `401`.

## Как устроена аутентификация

`AddKeycloakAuthentication` настраивает:

- confidential client с `ClientSecret`;
- Authorization Code Flow и PKCE;
- проверку `state` и `nonce` стандартным OIDC middleware;
- `openid profile email offline_access` scopes;
- сохранение access/refresh tokens через `SaveTokens=true`;
- cookie-сессию и realm/client role claims.

Порядок middleware: `UseAuthentication()` восстанавливает cookie-пользователя, затем `KeycloakJwtAuthenticationMiddleware` при наличии Bearer token заменяет principal результатом собственной JWT-проверки. Проверяются подпись RS256, JWKS, `iss`, `aud`, `exp`/`nbf` и `typ=Bearer`.

## Ограничения демо

Для production необходимы HTTPS, `CookieSecurePolicy.Always`, защищённое постоянное хранилище Data Protection keys и серверное хранилище сессий вместо размещения токенов в cookie. Автоматическое обновление access token по refresh token в этом примере не реализовано. Секреты нельзя фиксировать в репозитории.
