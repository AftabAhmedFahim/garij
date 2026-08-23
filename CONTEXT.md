# 🧠 AI & Developer Context — Garij Project

> **IMPORTANT INSTRUCTION FOR ALL AI ASSISTANTS:**
> Whenever you start working on this repository or finish a task:
> 1. Read this `CONTEXT.md` file to understand the current architecture, completed tasks, and team responsibilities.
> 2. Implement features on a dedicated feature branch (e.g., `feature/<task-name>`). Never commit directly to `main`.
> 3. After completing your work, **MUST UPDATE THIS `CONTEXT.md` FILE** with what was changed, any new decisions made, and the current state of the project.

---

## 📌 Project Overview
**Garij** is an Intelligent Vehicle Service Center Management System built with ASP.NET Core 10.0 MVC using a 3-layer architecture (Domain, Application, Infrastructure) + Presentation Layer (Web).

---

## 🛠️ Architecture & Tech Stack
- **Framework**: .NET 10.0 ASP.NET Core MVC
- **Architecture**: Layered Architecture (Clean separation of concerns)
  - `src/Garij.Domain`: Domain Entities, Enums, Exceptions (Zero dependencies).
  - `src/Garij.Application`: Service Interfaces, DTOs, Business Logic.
  - `src/Garij.Infrastructure`: EF Core `GarijDbContext`, Repositories, Identity, External APIs (Gemini).
  - `src/Garij.Web`: Controllers, Views, Middleware, Identity UI, Dependency Injection.
  - `tests/Garij.UnitTests`: Unit testing suite.
  - `tests/Garij.IntegrationTests`: Integration testing suite.
  - `tests/Garij.Tests`: General testing project.
- **Database**: Dual Provider Support:
  - **Local Development / Linux**: SQLite (`Data Source=Garij.db`) for zero-config cross-platform execution.
  - **Production / Windows**: SQL Server / LocalDB (`Server=...;Database=GarijDb`).
- **Authentication & Security**: ASP.NET Core Identity with role-based authorization (`Admin`, `Receptionist`, `Mechanic`, `Customer`).
- **Global Exception Middleware**: `GlobalExceptionMiddleware` catches uncaught exceptions, formats API/JSON responses for AJAX requests, and redirects to HTML error page for browser requests.

---

## 👥 Team Responsibility Matrix & Task Priorities

| Developer | Key Module Responsibilities | Priority Task | Status |
| :--- | :--- | :--- | :--- |
| **Emon** (Rakibul Islam Emon) | Foundation, Architecture, Identity, Vehicle Intake | Project Architecture Scaffolding & Exception Handling (Issue #1) | ✅ **Completed** |
| **Emon** (Rakibul Islam Emon) | Foundation, Architecture, Identity, Vehicle Intake | Database Schema Definition, EF Core Migrations & Seed Data (Issue #5) | ✅ **Completed** |
| **Aftab** (Aftab Ahmed) | Customer & Vehicle Management, Admin Dashboard, Reports | Customer & Vehicle Management UI/CRUD (Issue #2) | ⏳ Pending |
| **Rabib** (Rabib) | Mechanic Diagnostics, Parts Inventory, Billing, AI Intelligence | Diagnostic & Repair Log / Parts Inventory (Issue #3) | ⏳ Pending |
| **Samia** (Samia) | Public Portal, Customer Booking Lookup, Notifications, Testing | Customer Booking Status Lookup (Issue #4) | ⏳ Pending |

---

## 📝 Recent Progress Log

### [2026-08-24] - Database Schema Definition, EF Core Migrations & Seed Data (Emon - Priority 2)
- **Domain Entities**: Created `ApplicationUser.cs` and `AiRequestLog.cs` under `src/Garij.Domain/Entities/`.
- **Fluent API Configurations**: Created `AiRequestLogConfiguration.cs` and `ApplicationUserConfiguration.cs`. Enforced database check constraint for positive stock (`QuantityInStock >= 0` - BR-009) in `PartConfiguration.cs`, unique license plate index (BR-002) in `VehicleConfiguration.cs`, and 1:1 invoice cardinality (BR-012) in `InvoiceConfiguration.cs`.
- **GarijDbContext**: Updated `GarijDbContext.cs` with `ApplicationUsers` and `AiRequestLogs` DbSets.
- **EF Core Migrations**: Generated initial migration `InitialCreate` under `src/Garij.Infrastructure/Migrations/`.
- **DbSeeder**: Implemented `DbSeeder.cs` under `src/Garij.Infrastructure/SeedData/` and registered it in `Program.cs`. Automatically seeds Identity roles (`Admin`, `Receptionist`, `Mechanic`, `Customer`), Default Admin (`admin@garij.com` / `Admin@12345`), Service Catalog items, and stock parts.
- **Git Branching**: Created and pushed work to branch `feature/database-schema-and-seeder`.

### [2026-08-24] - Architecture Scaffolding & Exception Middleware (Emon - Priority 1)
- **Scaffolded Solution**: Generated `Garij.sln` containing all 7 core projects (`Web`, `Application`, `Domain`, `Infrastructure`, `UnitTests`, `IntegrationTests`, `Tests`).
- **Custom Exceptions**: Implemented `NotFoundException`, `ValidationException`, and `BusinessRuleException` under `Garij.Domain.Exceptions`.
- **Global Exception Middleware**: Built `GlobalExceptionMiddleware` under `Garij.Web.Middleware` and registered it in `Program.cs`. Supports both JSON API error responses and HTML error redirects.
- **Cross-Platform Database Setup**: Added EF Core SQLite provider fallback to `Garij.Infrastructure`. Configured `EnsureCreatedAsync` and auto-seeding of user roles in `Program.cs`.
- **Automated GitHub Issue Creation**: Scripted creation of 14 GitHub issues tracking feature development and assignees. Closed GitHub Issue #1.
- **Git Branching**: Created and pushed work to branch `feature/scaffolding-and-architecture`.

---

## 🏃 How to Run the Project

### 1. Build
```bash
dotnet build Garij.sln
```

### 2. Run Web Application
```bash
dotnet run --project src/Garij.Web
```
> The app will automatically initialize the database (`Garij.db`), seed default roles (`Admin`, `Receptionist`, `Mechanic`, `Customer`), and start listening on `http://localhost:5099` (or specified port).

### 3. Run Tests
```bash
dotnet test Garij.sln
```

---

## 🌿 Git Workflow Rules for All Developers & AI Assistants
1. **Branching**: Always checkout a new feature branch from `main`:
   ```bash
   git checkout -b feature/<your-feature-name>
   ```
2. **Never push directly to `main`**.
3. **Update `CONTEXT.md`**: Add your updates under "Recent Progress Log" when completing your task.
4. **Push Branch**: Push your branch to remote:
   ```bash
   git push origin feature/<your-feature-name>
   ```
