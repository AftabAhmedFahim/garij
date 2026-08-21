# Garij — Intelligent Vehicle Service Center Management System

Garij is a university group project: a web application for managing the day-to-day
operations of a vehicle service center — customer and vehicle records, service job
intake, mechanic assignment, parts inventory, billing, notifications, and reporting,
plus a public booking-status lookup for customers.

## Tech Stack

- **ASP.NET Core MVC** (.NET 10, LTS)
- **Entity Framework Core** (SQL Server / LocalDB)
- **ASP.NET Core Identity** for authentication and role-based authorization
- **xUnit** for testing
- Clean, layered architecture: Domain → Application → Infrastructure → Web

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- SQL Server LocalDB (installed with Visual Studio, or via [SQL Server Express LocalDB](https://learn.microsoft.com/sql/database-engine/configure-windows/sql-server-express-localdb))
- Visual Studio 2022 (17.12+), Rider, or VS Code with the C# Dev Kit

## Clone and Run

```bash
git clone <repository-url>
cd garij

# Restore and build
dotnet restore
dotnet build

# Apply database migrations (once migrations exist)
dotnet ef database update --project src/Garij.Infrastructure --startup-project src/Garij.Web

# Run the web app
dotnet run --project src/Garij.Web
```

The app will be available at the URL printed in the console (typically `https://localhost:5001`).

### Running the tests

```bash
dotnet test
```

## Folder Layout

```
Garij.slnx
src/
  Garij.Domain/           Entities and enums. No dependencies on other projects.
  Garij.Application/      Service interfaces, DTOs, and service implementations. Depends on Domain.
  Garij.Infrastructure/   EF Core DbContext, entity configurations, repositories. Depends on Domain, Application.
  Garij.Web/               ASP.NET Core MVC app: controllers, views, Identity, DI wiring. Depends on all of the above.
tests/
  Garij.Tests/             xUnit test project covering all layers.
.github/workflows/         CI pipeline (build + test on push/PR).
```
