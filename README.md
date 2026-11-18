# DalilAcademy — Learning Management System API

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API-512BD4?logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL-4169E1?logo=postgresql&logoColor=white)
![Architecture](https://img.shields.io/badge/Architecture-Onion-0A7B83)

An **ASP.NET Core Web API** for a Learning Management System (LMS). DalilAcademy provides course, student, and enrollment management, JWT-based authentication, role-based access, and a PayPal Sandbox payment workflow.

> The project is organized using Onion Architecture, with domain contracts kept separate from infrastructure and HTTP concerns.

## Contents

- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Domain and Payment Workflow](#domain-and-payment-workflow)
- [API Overview](#api-overview)
- [Getting Started](#getting-started)
- [PayPal Sandbox Configuration](#paypal-sandbox-configuration)
- [Validation and Error Handling](#validation-and-error-handling)
- [Build and CI](#build-and-ci)
- [Security Notes](#security-notes)

## Features

### Courses
- Course CRUD operations with instructor association.
- Paged course listing.
- Filter courses by minimum and maximum price.
- Search courses by title while retaining pagination.
- Course count endpoint.
- Course image upload and local image storage.
- Optional course capacity through `MaxStudents`.

### Students and Authentication
- Student management endpoints for administrators.
- Student registration and login.
- Password hashing with BCrypt.
- JWT authentication and role-based authorization for `Admin`, `Instructor`, and `Student`.

### Enrollments
- Create and manage student course enrollments.
- Enrollment lifecycle with `Pending` and `Paid` states.
- Free courses are marked `Paid` when the enrollment is created.
- Paid enrollments cannot be deleted or moved to another course.
- Students can view their own paid courses.
- Course student lists include paid enrollments only.

### Payments
- PayPal Orders API v2 integration using Sandbox credentials.
- Each payment attempt is stored independently.
- Payment states: `Created`, `Completed`, and `Failed`.
- Payment history for an enrollment, newest attempt first.
- The backend calculates the amount; the client never supplies the amount to charge.
- PayPal order capture is limited to the three-hour order window.

### Capacity and Search (Bonus Features)
- Only `Paid` enrollments count toward a course's `MaxStudents` limit.
- Course title search works together with pagination and price filters.
- Payment history exposes all payment attempts for an enrollment.

## Technology Stack

| Area | Technology |
|---|---|
| Framework | .NET 10, ASP.NET Core Web API |
| Language | C# |
| Data access | Entity Framework Core 10 |
| Database | PostgreSQL |
| ORM configuration | EF Core Fluent API and Code First migrations |
| Authentication | JWT Bearer authentication |
| Password hashing | BCrypt |
| Validation | FluentValidation |
| API documentation | Swagger / OpenAPI |
| Payments | PayPal Orders API v2 — Sandbox |
| HTTP integration | Typed `HttpClient` |
| CI | GitHub Actions |

## Architecture

The solution is split into three projects:

| Project | Responsibility |
|---|---|
| **DalilAcademy.Core** | Domain models and enums, DTOs, service and repository contracts, validation rules, security abstractions, and custom exceptions |
| **DalilAcademy.Infra** | EF Core `DbContext`, entity configurations, migrations, repository and service implementations, local image storage, and PayPal integration |
| **DalilAcademy.API** | REST controllers, dependency injection, JWT configuration, middleware, exception handling, and Swagger |

### Request and dependency flow

```text
HTTP Request
    |
    v
DalilAcademy.API
    |  Controller
    v
DalilAcademy.Core
    |  Service / Repository contracts
    v
DalilAcademy.Infra
    |  Service and repository implementations
    v
Entity Framework Core
    |
    v
PostgreSQL

PaymentService ----> IPaymentGateway ----> PayPalPaymentGateway
                                             |
                                             v
                                      PayPal Sandbox
```

Controllers are kept focused on HTTP concerns. Business rules live in services, while persistence and external PayPal communication are implemented in the infrastructure layer.

## Domain and Payment Workflow

### Enrollment status

| Situation | Enrollment status |
|---|---|
| New enrollment in a paid course | `Pending` |
| New enrollment in a free course | `Paid` |
| Successful PayPal capture | `Paid` |
| Failed or unapproved capture | Remains `Pending` |

A `Paid` enrollment cannot be deleted or transferred to another course. The student's **My Courses** endpoint returns paid courses only.

### PayPal payment lifecycle

1. An administrator creates an enrollment for a paid course. The enrollment is `Pending`.
2. The administrator calls `POST /api/enrollments/{id}/pay`.
3. The backend reads the course price in JOD, multiplies it by the configured JOD-to-USD rate (**1 JOD = 1.41 USD**), and rounds the result to two decimal places.
4. The API creates a PayPal order with intent `CAPTURE` and saves a `Payment` record with status `Created`.
5. The client opens the returned `approvalUrl`; the buyer approves the order using a PayPal Sandbox Personal account.
6. The administrator calls `POST /api/enrollments/{id}/capture`.
7. The enrollment becomes `Paid` only when PayPal reports `COMPLETED`. Otherwise the attempt is marked `Failed`, the enrollment remains `Pending`, and another payment attempt can be created.

Each attempt has its own payment record, so failed and previous attempts remain available through payment history. PayPal orders must be captured within three hours.

## API Overview

Swagger is available in the Development environment at:

`http://localhost:5118/swagger`

### Authentication

| Method | Route | Access | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Register a Student account |
| POST | `/api/auth/login` | Public | Authenticate and receive a JWT |

### Courses

| Method | Route | Access | Description |
|---|---|---|---|
| GET | `/api/courses` | Public | Paged course list; supports `search`, `minPrice`, and `maxPrice` |
| GET | `/api/courses/{id}` | Authenticated | Get a course |
| GET | `/api/courses/count` | Authenticated | Get the course count |
| GET | `/api/courses/{courseId}/students` | Admin | List students with a Paid enrollment in the course |
| POST | `/api/courses` | Admin | Create a course |
| PUT | `/api/courses/{id}` | Admin, Instructor | Update a course |
| DELETE | `/api/courses/{id}` | Admin | Delete a course |
| POST | `/api/courses/upload-image` | Admin | Upload a course image |

Example course search:

```http
GET /api/courses?search=net&page=1&pageSize=10
```

### Students

| Method | Route | Access | Description |
|---|---|---|---|
| GET | `/api/students` | Admin | Paged student list |
| GET | `/api/students/{id}` | Admin | Get a student |
| GET | `/api/students/{id}/courses` | Admin | Get a student's courses |
| POST | `/api/students` | Admin | Create a student |
| PUT | `/api/students/{id}` | Admin | Update a student |
| DELETE | `/api/students/{id}` | Admin | Delete a student |

### Enrollments and Payments

| Method | Route | Access | Description |
|---|---|---|---|
| GET | `/api/enrollments` | Admin | List enrollments |
| GET | `/api/enrollments/{id}` | Admin | Get one enrollment |
| GET | `/api/enrollments/me` | Student | Get the current student's Paid courses |
| GET | `/api/enrollments/{id}/payments` | Admin | List payment attempts, newest first |
| POST | `/api/enrollments` | Admin | Create an enrollment |
| POST | `/api/enrollments/{id}/pay` | Admin | Create a PayPal order and payment attempt |
| POST | `/api/enrollments/{id}/capture` | Admin | Capture the latest Created payment |
| PUT | `/api/enrollments/{id}` | Admin | Move an unpaid enrollment to another course |
| DELETE | `/api/enrollments/{id}` | Admin | Delete an unpaid enrollment |

All protected endpoints require a JWT Bearer token. Role restrictions are shown in the access column.

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL
- PayPal Developer account with Sandbox credentials (only required for testing payment endpoints)
- EF Core command-line tool

### 1. Clone the repository

```bash
git clone https://github.com/Abd-alrahman02/DalilAcademy.git
cd DalilAcademy
```

### 2. Configure secrets

The API project has a configured User Secrets ID. Run the following commands from the solution root and replace the sample values with your own local values.

**PowerShell:**

```powershell
$api = ".\DalilAcademy.API\DalilAcademy.API.csproj"

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=DalilAcademyDb;Username=postgres;Password=YOUR_DB_PASSWORD" --project $api
dotnet user-secrets set "Jwt:Key" "YOUR_RANDOM_SIGNING_KEY_AT_LEAST_32_BYTES_LONG" --project $api
dotnet user-secrets set "SeedAdmin:Password" "YOUR_LOCAL_ADMIN_PASSWORD" --project $api
dotnet user-secrets set "PayPal:ClientId" "YOUR_PAYPAL_SANDBOX_CLIENT_ID" --project $api
dotnet user-secrets set "PayPal:ClientSecret" "YOUR_PAYPAL_SANDBOX_CLIENT_SECRET" --project $api
```

Do not commit the values used in these commands. User Secrets are stored outside the repository.

### 3. Install the EF Core CLI tool

If it is not already installed:

```bash
dotnet tool install --global dotnet-ef --version 10.0.14
```

### 4. Restore, migrate, and build

```bash
dotnet restore DalilAcademy.slnx
dotnet ef database update --project DalilAcademy.Infra/DalilAcademy.Infra.csproj --startup-project DalilAcademy.API/DalilAcademy.API.csproj
dotnet build DalilAcademy.slnx --configuration Release
```

The database is created/updated from the committed EF Core migrations. On initial setup, the application also seeds an administrator and sample instructors/courses when the relevant tables are empty.

The seeded administrator email is `admin@dalil.jo`; its password is the value configured in `SeedAdmin:Password`.

### 5. Run the API

```bash
dotnet run --project DalilAcademy.API/DalilAcademy.API.csproj
```

The included HTTP launch profile uses `http://localhost:5118` and the Development environment. Open [Swagger UI](http://localhost:5118/swagger) to explore the endpoints.

## PayPal Sandbox Configuration

The following non-secret settings are stored in `DalilAcademy.API/appsettings.json`:

| Setting | Purpose |
|---|---|
| `PayPal:BaseUrl` | PayPal Sandbox API base URL |
| `PayPal:ReturnUrl` | URL PayPal redirects to after approval |
| `PayPal:CancelUrl` | URL PayPal redirects to if the buyer cancels |
| `PayPal:JodToUsdRate` | Fixed conversion rate; currently `1.41` |

The Client ID and Client Secret must be configured with User Secrets, never in `appsettings.json`.

Use the Sandbox **Business** account as the seller and the Sandbox **Personal** account as the buyer. Sandbox payments do not move real money. If the local API port changes, update the return and cancel URLs to match the Swagger address.

## Validation and Error Handling

The API uses FluentValidation for request validation and centralized exception handling with Problem Details responses.

Important business-rule responses include:

| Status | Example |
|---|---|
| 400 Bad Request | Capture is requested before a payment attempt exists, or PayPal does not complete the capture |
| 401 Unauthorized | Missing or invalid JWT |
| 403 Forbidden | Authenticated user does not have the required role |
| 404 Not Found | Requested course, student, or enrollment does not exist |
| 409 Conflict | Duplicate enrollment, an already-paid enrollment is modified, or course capacity is reached |
| 503 Service Unavailable | PayPal cannot be reached |

## Build and CI

A GitHub Actions workflow is configured at `.github/workflows/dotnet-build.yml`. It restores and builds the solution for pushes to `main` and `dev`, and for pull requests targeting `main`.

The workflow currently validates restore and compilation; it is not an end-to-end test of PostgreSQL or a live PayPal Sandbox payment.

## Security Notes

- Keep database credentials, JWT signing keys, the seeded administrator password, and PayPal credentials in User Secrets or another secure secret provider.
- Never commit local `secrets.json`, `.env` files containing credentials, or real production secrets.
- Use PayPal Sandbox credentials for local payment development.
- Use HTTPS and production-grade secret storage when deploying outside local development.
