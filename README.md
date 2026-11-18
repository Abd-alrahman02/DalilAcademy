# DalilAcademy LMS — Web API

An ASP.NET Core Web API for a Learning Management System, built with Onion Architecture.

## Technology

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core and PostgreSQL
- Fluent API configurations and EF Core migrations
- JWT authentication, role-based authorization, and BCrypt password hashing
- FluentValidation and centralized Problem Details error handling
- PayPal Orders API v2 (Sandbox)

## Solution structure

- **DalilAcademy.Core** — domain models, DTOs, repository/service contracts, validation, and exceptions
- **DalilAcademy.Infra** — EF Core persistence, repositories, business services, local image storage, and PayPal integration
- **DalilAcademy.API** — controllers, dependency injection, authentication, and middleware

## Run locally

Requirements: .NET 10 SDK and a running PostgreSQL instance.

### 1. Configure User Secrets

Run these commands from the solution root. The API project already has a User Secrets ID; initialize it only if your local project does not have one.

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<POSTGRES_CONNECTION_STRING>" --project .\DalilAcademy.API\DalilAcademy.API.csproj
dotnet user-secrets set "Jwt:Key" "<JWT_SIGNING_KEY_AT_LEAST_32_BYTES>" --project .\DalilAcademy.API\DalilAcademy.API.csproj
dotnet user-secrets set "SeedAdmin:Password" "<ADMIN_PASSWORD>" --project .\DalilAcademy.API\DalilAcademy.API.csproj
dotnet user-secrets set "PayPal:ClientId" "<PAYPAL_SANDBOX_CLIENT_ID>" --project .\DalilAcademy.API\DalilAcademy.API.csproj
dotnet user-secrets set "PayPal:ClientSecret" "<PAYPAL_SANDBOX_CLIENT_SECRET>" --project .\DalilAcademy.API\DalilAcademy.API.csproj
```

Never put database passwords, JWT signing keys, admin passwords, or PayPal credentials in tracked configuration files.

### 2. Apply database migrations

```powershell
dotnet ef database update --project .\DalilAcademy.Infra\DalilAcademy.Infra.csproj --startup-project .\DalilAcademy.API\DalilAcademy.API.csproj
```

### 3. Run the API

```powershell
dotnet run --project .\DalilAcademy.API\DalilAcademy.API.csproj
```

Swagger is available at `http://localhost:5118/swagger` when running with the included HTTP launch profile.

## Enrollment payment flow (PayPal Sandbox)

1. Sign in as an Admin and create an enrollment for a paid course. Its status is `Pending`.
2. Call `POST /api/enrollments/{id}/pay`. The server calculates the amount using the course price in JOD multiplied by the configured fixed rate of 1.41, rounds to two decimal places, creates a PayPal order, and stores a `Created` Payment record in USD.
3. Open the returned `approvalUrl` and approve the payment using the PayPal Sandbox personal (buyer) account.
4. Call `POST /api/enrollments/{id}/capture`. The enrollment becomes `Paid` only when PayPal returns `COMPLETED`. Failed attempts remain recorded as `Failed`, and a new payment attempt can be created with `/pay`.

The PayPal return and cancel URLs are configured in `appsettings.json`. Update them if your local Swagger address changes. Sandbox credentials belong in User Secrets only; Sandbox does not move real money.

## Main endpoints

| Method | Route | Authorization | Purpose |
|---|---|---|---|
| GET | `/api/courses` | Public | Paged course listing and price filters |
| GET | `/api/courses/{courseId}/students` | Admin | Students with Paid enrollments in a course |
| GET | `/api/enrollments` | Admin | List enrollments |
| GET | `/api/enrollments/{id}` | Admin | Get one enrollment |
| POST | `/api/enrollments` | Admin | Create a Pending enrollment, or Paid if the course is free |
| PUT | `/api/enrollments/{id}` | Admin | Change the enrollment's course (Paid enrollments cannot be moved) |
| DELETE | `/api/enrollments/{id}` | Admin | Delete an unpaid enrollment |
| GET | `/api/enrollments/me` | Student | View the current student's Paid courses |
| POST | `/api/enrollments/{id}/pay` | Admin | Create a PayPal order |
| POST | `/api/enrollments/{id}/capture` | Admin | Capture the latest Created payment |

All amounts are calculated by the backend. The API never accepts the amount to charge from the client.
