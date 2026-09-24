# Job Application Platform API

A robust, enterprise-ready ASP.NET Core Web API built with **Clean Architecture**, **CQRS with MediatR**, **Entity Framework Core with SQL Server**, **JWT Authentication & RBAC**, and **Hangfire Background Job Processing**.

---

## 🏛 Architecture Overview

The solution adheres strictly to **Clean Architecture** principles, maintaining clear separation of concerns across four layers:

```
├── JopApplicationPlatform.Domain         # Enterprise entities, enums, domain constants
├── JopApplicationPlatform.Application    # Use cases, CQRS commands/queries, MediatR handlers, DTOs, interfaces
├── JopApplicationPlatform.Infrastructure # EF Core DbContext, repository implementations, migrations
└── JopApplicationPlatform.API            # ASP.NET Core controllers, middleware, Hangfire dashboard, OpenAPI
```

- **Domain**: Contains core domain models (`Job`, `JobApplication`, `User`, `Candidate`), `JobApplicationStatus` enum, and role constants (`Recruiter`, `Candidate`).
- **Application**: Implements the CQRS pattern via MediatR. Contains request/response DTOs, interface abstractions (`IRepository<T>`, `IJobBackgroundService`, `IJobService`), and business logic handlers.
- **Infrastructure**: Manages persistence with SQL Server via `ApplicationDbContext`, generic `Repository<T>`, and EF Core code-first migrations.
- **API**: Hosts RESTful endpoints, JWT authentication configuration, Hangfire server/dashboard, and Scalar API documentation.

---

## ⚡ CQRS & MediatR

All business workflows are organized into commands and queries:

### Commands (State Mutations)
- **`RegisterCommand`**: Registers a user with BCrypt hashed passwords and auto-creates Candidate profiles for candidate roles.
- **`LoginCommand`**: Validates credentials and generates HMAC-SHA256 signed JWT tokens containing user ID, email, and role claims.
- **`CreateJobCommand`**: Allows Recruiters to publish new job listings with recruiter ownership tracking.
- **`CancelJobCommand`**: Allows Recruiters to close/deactivate their own job postings.
- **`ApplyToJobCommand`**: Allows Candidates to apply for active jobs, preventing duplicate applications and triggering Hangfire background jobs.
- **`CancelApplicationCommand`**: Allows Candidates to cancel their own submitted applications.

### Queries (Read Operations)
- **`GetAvailableJobsQuery`**: Returns all active job listings (accessible anonymously).
- **`GetJobByIdQuery`**: Fetches details of a specific job by ID.
- **`GetApplicantsForJobQuery`**: Allows Recruiters to view all applications submitted for a job they posted.
- **`GetMyApplicationsQuery`**: Allows Candidates to review the status of their own applications.

---

## 🕒 Hangfire Background Processing

The application leverages **Hangfire** backed by **SQL Server** for resilient background job execution.

### 1. Recurring Job: `auto-close-expired-jobs`
- **Method**: `IJobBackgroundService.AutoCloseExpiredJobsAsync()`
- **Job ID**: `auto-close-expired-jobs`
- **Cron Expression**: `0 0 * * *` (`Cron.Daily`)
- **Meaning**: Executes every day at midnight (00:00 UTC).
- **Business Logic**:
  - Scans for active jobs that have passed the retention/expiration window.
  - Automatically deactivates the jobs (`IsActive = false`), sets `ClosedAt = DateTime.UtcNow`, and assigns `ClosedBy = 0` (indicating system automated action).
  - Updates any pending applications (`Applied` or `UnderReview`) for closed jobs to `Rejected` and updates `StatusUpdatedAt`, ensuring candidates are not left waiting indefinitely.
  - Visible under **Hangfire Dashboard → Recurring Jobs**.

### 2. Fire-and-Forget Job: Application Submission Notification
- **Trigger**: Automatically enqueued in `ApplyToJobHandler` immediately after an application is committed to the database.
- **Method**: `IJobBackgroundService.SendApplicationSubmittedNotificationAsync(applicationId)`
- **Business Logic**: Simulates delivering a confirmation email to the candidate and notifying the recruiter responsible for the job.

### 3. Delayed Job: Recruiter Review Reminder
- **Trigger**: Automatically scheduled in `ApplyToJobHandler` when an application is submitted.
- **Method**: `IJobBackgroundService.SendApplicationReviewReminderAsync(applicationId)`
- **Delay**: Scheduled for 24 hours (`TimeSpan.FromHours(24)`).
- **Business Logic**: Checks if the application is still in `Applied` status after 24 hours; if unreviewed, delivers a review reminder to the recruiter.

---

## 🔐 Authentication & Role-Based Authorization (RBAC)

The API uses JWT Bearer Authentication with role-based policies:
- **`Recruiter`**: Can create jobs, close/cancel jobs they posted, and view applicant lists for their postings.
- **`Candidate`**: Can apply to active jobs, view their submitted applications, and cancel their own applications.
- **`Anonymous`**: Can view available jobs and query individual job postings.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (LocalDB or SQL Server Express / Developer)

### Configuration
Database connection and JWT configuration are located in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=(localdb)\\ProjectModels;Initial Catalog=JopApplicationPlatform;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=False;"
  },
  "JwtSettings": {
    "Key": "super_secret_key_that_is_at_least_32_bytes_long_12345!"
  }
}
```

### Apply Database Migrations
```powershell
dotnet ef database update --project JopApplicationPlatform.Infrastructure --startup-project JopApplicationPlatform.API
```

### Run the Application
```powershell
dotnet run --project JopApplicationPlatform.API --launch-profile http
```
The API starts listening on `http://localhost:5080`.

---

## 📊 Dashboards & Documentation

| Tool | URL | Description |
|---|---|---|
| **Hangfire Dashboard** | `http://localhost:5080/hangfire` | Monitor background jobs, recurring jobs, and server stats |
| **Recurring Jobs** | `http://localhost:5080/hangfire/recurring` | View and manually trigger recurring jobs |
| **Scalar API Reference** | `http://localhost:5080/scalar/v1` | Interactive modern API documentation and testing |
| **OpenAPI Spec** | `http://localhost:5080/openapi/v1.json` | Raw OpenAPI v3 specification |

---

## 🧪 Example API Flow

### 1. Register a Recruiter
```http
POST /api/Auth/register
Content-Type: application/json

{
  "email": "recruiter@techcorp.com",
  "password": "Password123!",
  "role": "Recruiter"
}
```

### 2. Login as Recruiter
```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "recruiter@techcorp.com",
  "password": "Password123!"
}
```
*Copy the returned `token` for subsequent requests.*

### 3. Create a Job
```http
POST /api/Jobs
Authorization: Bearer <RECRUITER_TOKEN>
Content-Type: application/json

{
  "title": "Senior .NET Backend Engineer",
  "description": "Building high-performance distributed systems with Hangfire and CQRS.",
  "isActive": true
}
```

### 4. Register a Candidate & Apply
```http
POST /api/Auth/register
Content-Type: application/json

{
  "email": "candidate@example.com",
  "password": "Password123!",
  "role": "Candidate"
}
```

```http
POST /api/Jobs/1/apply
Authorization: Bearer <CANDIDATE_TOKEN>
```
*This automatically enqueues the Hangfire notification and schedules the 24-hour review reminder!*

### 5. Check Candidate Applications
```http
GET /api/Applications/my
Authorization: Bearer <CANDIDATE_TOKEN>
```

### 6. View Applicants (Recruiter Only)
```http
GET /api/Jobs/1/applications
Authorization: Bearer <RECRUITER_TOKEN>
```