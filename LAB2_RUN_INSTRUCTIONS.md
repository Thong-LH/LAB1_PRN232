# PRN232 LMS Lab 2 Run Instructions

## Run locally

```powershell
dotnet run --project PRN232.LMS.API\PRN232.LMS.API.csproj --urls http://localhost:5283
```

Swagger:

```text
http://localhost:5283/swagger
```

## Run with Docker

Start Docker Desktop first, then run:

```powershell
docker compose up --build
```

Swagger:

```text
http://localhost:8080/swagger
```

The Docker configuration recreates the database on startup and seeds Lab 1 data plus the admin user.

## Default admin account

```text
username: admin
password: 123456
role: Admin
```

## Login

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "123456"
}
```

Copy `data.accessToken`, open Swagger **Authorize**, and paste the token.

## Refresh token

```http
POST /api/v1/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "<refresh token from login response>"
}
```

## Protected API examples

```text
GET /api/v1/courses?expand=semester&page=1&size=10
GET /api/v1/courses/1/enrollments?expand=student
DELETE /api/v1/students/1
```

The delete endpoints require an Admin JWT.

## Content negotiation

JSON:

```http
Accept: application/json
```

XML:

```http
Accept: application/xml
```

Unsupported formats return HTTP 406.
