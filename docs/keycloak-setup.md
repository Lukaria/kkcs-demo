# Настройка Keycloak: confidential client

Инструкция соответствует ветке `confidential` и Keycloak 26. Используются realm `kkcs-demo`, клиент и audience `kkcs-api`.

## 1. Вход в Admin Console

1. Выполните `docker compose up -d` в корне решения.
2. Откройте <http://localhost:8080/admin>.
3. Войдите с локальными демонстрационными учётными данными `admin` / `admin`.

## 2. Создание realm

1. Откройте список realm и нажмите **Create realm**.
2. Укажите **Realm name**: `kkcs-demo`.
3. Нажмите **Create** и выберите созданный realm.

Issuer: `http://localhost:8080/realms/kkcs-demo`.

## 3. Создание confidential-клиента

1. Перейдите в **Clients** → **Create client**.
2. Оставьте **Client type** = `OpenID Connect`, укажите **Client ID** = `kkcs-api`.
3. На странице **Capability config**:
   - включите **Client authentication**;
   - включите **Standard flow**;
   - отключите **Direct access grants**;
   - отключите **Implicit flow**;
   - оставьте **Service accounts roles** выключенным: вход выполняется от имени пользователя.
4. Нажмите **Next**, затем **Save**.

Client authentication означает, что сервер подтверждает личность клиента секретом при обмене authorization code на токены. Authorization Code Flow заменяет Direct Access Grants: пароль пользователя приложению недоступен.

## 4. Redirect URI и PKCE

На странице **Settings** клиента укажите:

- **Valid redirect URIs**: `http://localhost:5273/signin-oidc`;
- **Valid post logout redirect URIs**: `http://localhost:5273/signout-callback-oidc`;
- **Web origins**: `http://localhost:5273` либо оставьте пустым, если CORS не требуется.

В **Advanced** включите **Proof Key for Code Exchange Code Challenge Method** = `S256`, если этот параметр доступен. Приложение в любом случае отправляет PKCE (`UsePkce=true`).

Не используйте `*` или `/*` в production redirect URI.

## 5. Client secret

1. Откройте **Clients** → `kkcs-api` → **Credentials**.
2. Скопируйте **Client secret**.
3. Сохраните его в локальном User Secrets приложения:

   ```bash
   dotnet user-secrets set "Keycloak:ClientSecret" "ВАШ_СЕКРЕТ" --project src/Kkcs.KeycloakDemo.Api
   ```

Не записывайте секрет в `appsettings.json` и не отправляйте его в репозиторий. Пустое значение секрета приведёт к ошибке проверки конфигурации при запуске.

## 6. Audience для Bearer API

Этот mapper нужен для вызовов `/me` с внешним Bearer access token. Для OIDC cookie-входа он не требуется.

1. Откройте **Clients** → `kkcs-api` → **Client scopes**.
2. Откройте `kkcs-api-dedicated`.
3. Выберите **Configure a new mapper** → **Audience**.
4. Укажите имя `kkcs-api-audience`.
5. В **Included Client Audience** выберите `kkcs-api`.
6. Оставьте **Add to access token** включённым и сохраните.

Без mapper собственный middleware отклонит Bearer token с неподходящим `aud`.

## 7. Роли

1. Откройте **Clients** → `kkcs-api` → **Roles** → **Create role**.
2. Создайте `demo-user` и при необходимости `demo-admin`.
3. Откройте пользователя → **Role mapping** → **Assign role**.
4. Выберите **Filter by clients**, затем `kkcs-api` и нужную роль.

OIDC-обработчик и собственный JWT middleware преобразуют realm/client roles в `ClaimTypes.Role`.

## 8. Пользователь и обязательный профиль

1. Перейдите в **Users** → **Add user**.
2. Укажите **Username**, например `demo-user`.
3. Заполните **Email**, **First name** и **Last name**. В Keycloak 26 эти поля могут быть обязательными в **Realm settings → User profile**, даже если список **Required user actions** пуст.
4. Включите **Email verified** и нажмите **Create**.
5. На вкладке **Credentials** нажмите **Set password**.
6. Установите пароль, отключите **Temporary** и сохраните.

Если обязательные поля профиля пусты, Keycloak может вернуть `400 invalid_grant: Account is not fully set up`.

## 9. Проверка браузерного входа

1. Запустите API и откройте <http://localhost:5273/login>.
2. На странице Keycloak введите логин и пароль пользователя.
3. Keycloak перенаправит на `/signin-oidc`; приложение проверит `state`, `nonce` и PKCE, обменяет code на токены с `client_secret` и создаст cookie-сессию.
4. После возврата `/me` должен показать пользователя и роли.
5. Откройте `/logout`: локальная cookie-сессия и Keycloak-сессия будут завершены.
6. `/uptime` доступен без входа.

Access и refresh tokens не отображаются в браузере. Scope `offline_access` запрашивает refresh token, а `SaveTokens=true` сохраняет токены в authentication session.

## Production

Используйте HTTPS, точные redirect URI, короткий срок жизни access token, ротацию секретов и безопасное хранилище Data Protection keys. Для больших токенов и нескольких экземпляров API используйте серверное хранилище сессий. Реализуйте обновление access token по refresh token до истечения срока его действия и политику отзыва сессий.
