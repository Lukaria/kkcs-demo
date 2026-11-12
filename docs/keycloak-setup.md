# Настройка Keycloak: пользователь, клиент и права

Инструкция соответствует `docker-compose.yml` из этого проекта и Keycloak 26. Настройки API по умолчанию: realm `kkcs-demo`, клиент и audience `kkcs-api`.

## 1. Вход в Admin Console

1. Выполните `docker compose up -d` в корне решения.
2. Дождитесь запуска контейнера и откройте <http://localhost:8080/admin>.
3. Войдите с временными локальными учётными данными `admin` / `admin`.

Эти учётные данные предназначены только для локальной демонстрации.

## 2. Создание realm

1. Откройте список realm в левом верхнем углу и нажмите **Create realm**.
2. Укажите **Realm name**: `kkcs-demo`.
3. Нажмите **Create** и убедитесь, что realm `kkcs-demo` выбран.

Issuer будет иметь значение `http://localhost:8080/realms/kkcs-demo`.

## 3. Создание клиента API

1. Перейдите в **Clients** → **Create client**.
2. Оставьте **Client type** = `OpenID Connect`, укажите **Client ID** = `kkcs-api` и нажмите **Next**.
3. Отключите **Client authentication**: демо использует публичный клиент, поэтому `ClientSecret` в `appsettings.json` остаётся пустым.
4. Включите **Direct access grants**. **Standard flow** для этого демо можно отключить.
5. Нажмите **Next**, затем **Save**. Redirect URI для Direct Access Grants не нужны.

Если выбран confidential-клиент, дополнительно скопируйте секрет из вкладки **Credentials** и передайте его через конфигурацию `Keycloak:ClientSecret`; не сохраняйте реальный секрет в репозитории.

## 4. Настройка audience

Middleware принимает только токен, у которого `aud` содержит `kkcs-api`. Добавьте соответствующий mapper:

1. Откройте **Clients** → `kkcs-api` → **Client scopes**.
2. Откройте dedicated scope `kkcs-api-dedicated`.
3. Выберите **Configure a new mapper** → **Audience**. Если список уже открыт, используйте **Add mapper** → **By configuration** → **Audience**.
4. Укажите имя, например `kkcs-api-audience`.
5. В **Included Client Audience** выберите `kkcs-api`.
6. Оставьте **Add to access token** включённым и нажмите **Save**.

Без этого mapper Keycloak обычно выдаёт access token только с audience `account`, поэтому `/me` вернёт `401`.

## 5. Создание ролей

Client roles позволяют выдавать права конкретного API:

1. Откройте **Clients** → `kkcs-api` → **Roles** → **Create role**.
2. Создайте роль `demo-user`, затем при необходимости `demo-admin`.

При необходимости можно также создать realm role через **Realm roles** → **Create role**. Middleware добавляет в principal оба типа ролей. Для авторизации по конкретному клиенту предпочтительны client roles.

## 6. Создание пользователя

1. Перейдите в **Users** → **Add user**.
2. Укажите **Username** = `demo-user`, при желании заполните email, имя и фамилию.
3. Включите **Email verified**, если задан email, и нажмите **Create**.
4. На вкладке **Credentials** нажмите **Set password**.
5. Укажите пароль, например `Demo123!`, отключите **Temporary** и подтвердите установку.

## 7. Назначение прав пользователю

1. Откройте **Users** → `demo-user` → **Role mapping** → **Assign role**.
2. Переключите фильтр на **Filter by clients**.
3. Выберите `kkcs-api` и роль `demo-user`, затем нажмите **Assign**.
4. Для назначения realm role используйте фильтр **Filter by realm roles**.

После входа `GET /me` вернёт назначенные роли в массиве `roles`. Сами роли не заменяют политику авторизации: защищаемый эндпоинт может дополнительно использовать `.RequireAuthorization(policy => policy.RequireRole("demo-admin"))`.

## 8. Проверка

1. Запустите API и откройте <http://localhost:5273/swagger>.
2. Вызовите `POST /login` с `demo-user` и установленным паролем.
3. Скопируйте `accessToken` из ответа, нажмите **Authorize** и вставьте токен без `Bearer`.
4. Вызовите `GET /me` — ожидаются `200` и данные пользователя.
5. Вызовите `GET /uptime` без Authorize — ожидается `200`.
6. Вызовите `POST /logout`, передав `refreshToken`, — ожидается `204`.

После logout ранее выданный access token может оставаться криптографически действительным до истечения `exp`: JWT проверяется локально. Refresh token и пользовательская сессия при этом уже завершены. Для немедленного отзыва access token нужен отдельный механизм introspection/revocation-list, который в это демо не входит.

## Production

Direct Access Grants выбран только для наглядного локального демо. В реальном приложении используйте Authorization Code Flow + PKCE, HTTPS, короткий срок жизни access token, безопасное хранение секретов и отдельные least-privilege роли. Значение `RequireHttpsMetadata` должно быть `true`.
