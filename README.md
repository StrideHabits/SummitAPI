# SummitAPI

SummitAPI is a simple ASP.NET Core 8 REST API for habit tracking.
It powers the **StrideHabits** Android app and provides:

* **SQLite** for lightweight persistence
* **JWT-based authentication**
* **File uploads** for habit-related images and assets

---

## 🚀 Features

* **Users**

  * Register and login
  * JWT issued on successful authentication

* **Habits**

  * Create new habits
  * List existing habits for the authenticated user

* **Check-ins**

  * Daily completion tracking
  * Idempotent behaviour (no duplicate check-ins for the same day and habit)

* **Settings**

  * Get user preference settings
  * Update preferences per user

* **Uploads**

  * Upload files (e.g., images)
  * Static serving from `/uploads/*`

---

## 🧩 Stack

* **Runtime**: .NET 8 / ASP.NET Core 8
* **Database**: SQLite
* **Auth**: JWT Bearer tokens
* **ORM**: Entity Framework Core
* **API Docs**: Swagger / Swashbuckle

---

## ✅ Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/)
* EF Core tools (optional, for migrations):

  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## 🛠 Setup & Running

From the project root:

```bash
dotnet restore
dotnet ef database update
dotnet run
```

By default, Swagger UI will be available at:

```text
https://localhost:<port>/swagger
```

---

## 🔐 Configuration – JWT Secrets

SummitAPI requires JWT configuration. Add the following to **User Secrets** or `appsettings.json`:

```json
"Jwt": {
  "Issuer": "SummitApi",
  "Key": "LONG_RANDOM_SECRET"
}
```

Notes:

* `Issuer` must match the value the API expects (`SummitApi` by default).
* `Key` must be a sufficiently long, random secret string.
* Without these values, login and all authenticated endpoints will fail.

---

## 🔄 Typical API Flow

1. **Register**

   * `POST /api/users/register`
   * Create a new user account.

2. **Login**

   * `POST /api/users/login`
   * On success, receive a JWT token.

3. **Authorize (Swagger)**

   * Open Swagger UI → click the Authorization button.
   * Paste the JWT token as: `Bearer <your_token_here>`.

4. **Call secured endpoints**

   * All endpoints that require authentication will now accept your token.

---

## 📸 Screenshots

SQLite DB connected:

<img width="698" alt="sqlite" src="https://github.com/user-attachments/assets/3eca1987-372e-400b-a781-fbf99b5d1fd7" />

Swagger UI working:

<img width="1416" alt="swagger" src="https://github.com/user-attachments/assets/e7696153-daf8-418b-a70a-75bac131bf0e" />

---

## 👤 Contributors – SummitAPI

SummitAPI backend development:

| Name            | Role                          |
| --------------- | ----------------------------- |
| **Dean Gibson** | Backend developer, API design |

---

## 👥 Contributors – StrideHabits Android App

StrideHabits is the Android client that consumes SummitAPI.

| Name                 | Student Number |
| -------------------- | -------------- |
| **Musa Ntuli**       | ST1029336      |
| **Dean Gibson**      | ST10326084     |
| **Fortune Mabona**   | ST10187287     |
| **Matthew Pieterse** | ST10257002     |

---
