# SummitAPI — Test

Simple ASP.NET Core 8 API for habits tracking. Uses **SQLite**, **JWT auth**, and basic file uploads.

---

## Features

* **Users**: Register & login (JWT issued).
* **Habits**: Create and list.
* **Check-ins**: Daily completion, idempotent.
* **Settings**: Get & update user preferences.
* **Uploads**: File upload, served from `/uploads/*`.

---

## Running

```bash
dotnet restore
dotnet ef database update
dotnet run
```

Swagger at: `https://localhost:<port>/swagger`

---

## Secrets (required)

Add to **User Secrets** or `appsettings.json`:

```json
"Jwt": {
  "Issuer": "SummitApi",
  "Key": "LONG_RANDOM_SECRET"
}
```

Without these, login & auth endpoints will fail.

---

## Flow

1. Register (`POST /api/users/register`)
2. Login (`POST /api/users/login`) → copy token
3. Authorize in Swagger (green lock → paste token)
4. Call secured endpoints

---

## Screenshots

SQLite DB connected: <img width="698" alt="sqlite" src="https://github.com/user-attachments/assets/3eca1987-372e-400b-a781-fbf99b5d1fd7" />

Swagger UI working: <img width="1416" alt="swagger" src="https://github.com/user-attachments/assets/e7696153-daf8-418b-a70a-75bac131bf0e" />
