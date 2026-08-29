# 🧠 AI & Developer Context — Garij Project

> **IMPORTANT INSTRUCTION FOR ALL AI ASSISTANTS:**
> Whenever you start working on this repository or finish a task:
> 1. Read this `CONTEXT.md` file alongside `ROADMAP.md` to understand the current architecture, completed tasks, and team responsibilities.
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
- **Database**: **Microsoft SQL Server (MS SQL)**:
  - **Standard Database**: Microsoft SQL Server / LocalDB (`Server=...;Database=GarijDb;...`).
  - **Fallback Development Support**: SQLite (`Data Source=Garij.db`) for zero-config Linux development environments when specified.
- **Authentication & Security**: ASP.NET Core Identity with role-based authorization (`Admin`, `Receptionist`, `Mechanic`, `Customer`).
- **Global Exception Middleware**: `GlobalExceptionMiddleware` catches uncaught exceptions, formats API/JSON responses for AJAX requests, and redirects to HTML error page for browser requests.

---

## 👥 Team Responsibility Matrix & Vertical Slice Ownership (Aligned with ROADMAP.md)

| Developer | Owned Modules | Primary Files / Interfaces | Stage 1 Core Focus |
| :--- | :--- | :--- | :--- |
| **Samia Tabassum** | Customer & Vehicle, Public Status Lookup | `CustomerVehicleService`, `CustomerController`, `VehicleController`, `StatusLookupController` | Customer registration, vehicle lookup, license plate validation |
| **Rakibul Islam Emon** | Service Jobs, Mechanic Assignment | `ServiceJobService`, `ServiceJobController`, `MechanicController` | Service job creation, mechanic assignment, Service Catalog seeding (`GRJ-2026-XXXX`) |
| **Rubaiat Ar Rabib** | Parts & Inventory, Notifications | `PartsInventoryService`, `NotificationService`, `PartsController`, `NotificationController` | Part stock management, 15-20 part seeding, reorder level alerts |
| **Aftab Ahmed Fahim** | Billing & Invoicing, Reporting, Intelligence | `BillingService`, `ReportingService`, `IntelligenceService`, `AccountController`, `DashboardController` | Auth/Identity setup, role navigation (`_Layout.cshtml`), Invoice generation skeleton |

---

## 📋 Roadmap & GitHub Issues Breakdown

### Stage 0 — Blockers (Immediate Focus)
1. **Identity Model Consolidation**: Finalize single Identity User model or proper FK link to `AspNetUsers`.
2. **Customer → ServiceJob Link**: Reconcile direct link vs vehicle link.
3. **Default Route**: Update default route to `Dashboard` / `Account/Login`.
4. **Solution File Alignment**: Standardize on `Garij.sln` across build scripts and CI pipelines.
5. **Initial Migration & Seeding**: Generate initial EF Core migration targeting MS SQL Server.

### Stage 1 — Foundation & Core CRUD
- **Issue #1 (Samia)**: Implement `CustomerVehicleService` & CRUD Views (`CustomerController`, `VehicleController`).
- **Issue #2 (Emon)**: Implement `ServiceJobService` & Job Booking Reference Generation (`ServiceJobController`, `MechanicController`).
- **Issue #3 (Rubaiat)**: Implement `PartsInventoryService` & Stock Validation (`PartsController`).
- **Issue #4 (Aftab)**: Account Controller, Auth Guard, Dashboard Routing & Billing Skeleton (`AccountController`, `BillingService`).

### Stage 2 — Business Logic & Workflows
- **Issue #5 (Samia)**: Public Status Lookup Timeline View & Service History (`StatusLookupController`).
- **Issue #6 (Emon)**: Job Status State Machine & Diagnostic Log (`ValidateStatusTransition`).
- **Issue #7 (Rubaiat)**: Log Parts Used, Atomic Stock Decrement & Notification Approval Queue (`LogPartsUsed`).
- **Issue #8 (Aftab)**: Transactional Invoice Generation & Multi-Method Payment Recording (`GenerateInvoice`).

### Stage 3 — Intelligence, Reporting & Polish
- **Issue #9 (Samia)**: UI/UX Pass, Color-Coded Job Board Statuses & Mobile Optimization.
- **Issue #10 (Emon)**: Service-Due Vehicle Flagging & Advanced Job Board Filtering.
- **Issue #11 (Rubaiat)**: Stock Concurrent Decrement Unit Tests & Presentation Demo Dataset.
- **Issue #12 (Aftab)**: Smart Intake Gemini AI Assistant & Executive Reports (`IReportingService`).

---

## 📝 Recent Progress Log

### [2026-08-29] - Landing Page UI Redesign & High-Tech Automotive Overhaul (Aftab Ahmed Fahim)
- **Public Landing Page (HomeController & _LandingLayout)**: Created `HomeController` (`[AllowAnonymous]`) mapped to the default route `/`, with dedicated full-bleed dark automotive layout `_LandingLayout.cshtml` and view `Views/Home/Index.cshtml`.
- **Visual Design & Aesthetics (Demo Mockup Alignment)**:
  - Top contact info bar with 24/7 phone (`0-800-123-4567`), email (`Carrepair@example.com`), and Miami location (`9332 Bernier Dam, Miami, USA`).
  - Dark cinematic hero section featuring vintage muscle car in atmospheric smoke, winged crossed spark-plug repair badge ("REPAIR SERVICE 1983 / TUNING"), uppercase Montserrat headline "CREATIVE & PROFESSIONAL", and glowing "READ MORE" button.
  - "YOU NEED RENOVATION?" section with geometric blue border framing, descriptive copy, 3D tuned sports car with glowing blue neon wheel rims and underglow, and 3 feature cards (25 Years Reputation, Full Integrity, Quick & Efficient).
  - Testimonials section with winged emblem and 3-column client review cards (Bill Alvarado, Alberta Wilson, Zachary Fernandez).
  - Modern automotive footer with workshop operating hours, quick links, and copyright.
- **Interactive Animations & Preloader**:
  - Tachometer/Speedometer revving loading animation (`#page-preloader`) on initial page load.
  - IntersectionObserver scroll reveal animations (`reveal-left`, `reveal-right`, `reveal-up`) across cards and sections.
  - Glowing hover effects with animated expanding underlines on navigation links and dedicated neon buttons for **Status Lookup**, **Register**, and **Login**.
- **Typography & Asset Organization**:
  - Configured Montserrat typography (Regular 400, Italic 400i, Semi-Bold 600, Bold 700/800) with both local `@font-face` support in `wwwroot/fonts/` and Google Fonts fallback.
  - Structured `wwwroot/images/` for user assets: `hero-car.png`, `renovation-car.png`, `repair-badge.svg`, and `testimonial-badge.svg`.
- **Testing & Verification**: Verified build (`dotnet build Garij.sln`), 100% unit and integration test pass rate (52/52 tests in `dotnet test Garij.sln`), and verified in browser.

### [2026-08-29] - Stage 2 Business Logic & Mechanic Workflows (Rakibul Islam Emon)
- **Job Status State Machine (FR-7)**: Implemented strict status state machine transition validation in `ServiceJobService.ValidateStatusTransition`. Allowed flow: `Requested` &rarr; `InspectionPending` &rarr; `CustomerApprovalNeeded` &rarr; `InProgress` &rarr; `Completed`. `Cancelled` is accessible from any active non-completed state. Invalid transitions trigger `BusinessRuleException` ("BR-007").
- **Logged Parts Completion Pre-Condition (FR-8)**: Implemented hard business rule in `ServiceJobService` preventing jobs from transitioning to `Completed` status unless at least one part used is recorded in `JobPartsUsed`. Rejections throw `BusinessRuleException` ("BR-008").
- **Lead Mechanic Uniqueness Constraint (FR-3)**: Enforced single Lead mechanic constraint in `AssignMechanicAsync`. Attempting to assign a second lead technician throws `BusinessRuleException` ("BR-003").
- **Touch-Friendly Mechanic Job Board (FR-10)**: Created `MechanicController.JobBoard` and touch-optimized view (`Views/Mechanic/JobBoard.cshtml`) featuring responsive cards, mechanic filter dropdown, inline diagnostic notes editor, log parts quick link, and status action buttons.
- **Repository & Service Contracts**: Extended `IServiceJobRepository`, `ServiceJobRepository`, `IServiceJobService`, and `ServiceJobService` with `GetJobsByMechanicAsync(int mechanicUserId)` and `SaveDiagnosticNotesAsync(int serviceJobId, string notes)`.
- **Navigation Layout Update**: Added "Job Board" link to top navigation bar in `_Layout.cshtml` for authenticated mechanics, front desk staff, and admins.
- **Comprehensive Unit Testing**: Expanded `ServiceJobServiceTests.cs` with test cases for valid transitions, invalid transition rejections, parts completion guards, and lead mechanic uniqueness. Verified all 48 solution unit/integration tests pass (`dotnet test`).
- **Git Branching**: Committed and pushed changes to remote branch `feature/business-logic-mechanic-workflows` using conventional commits.

### [2026-08-26] - Customer Direct Link Reconciliation, Default Route Update & Solution Cleanup
- **Customer → ServiceJob Direct Link**: Added `CustomerId` FK and `Customer` navigation property to `ServiceJob.cs` and `ServiceJobs` collection to `Customer.cs` matching report ERD. Configured `FK_ServiceJobs_Customers_CustomerId` in `ServiceJobConfiguration`.
- **Default Route Landing**: Updated default route in `Program.cs` to `Dashboard` (`{controller=Dashboard}/{action=Index}`), routing unauthenticated users to `/Account/Login`.
- **Solution Standardization**: Removed duplicate `Garij.slnx` file to standardize build tools exclusively on `Garij.sln`.
- **EF Core Migration & Verification**: Re-generated MS SQL EF Core initial migration `20260826093448_InitialCreate`. All unit and integration tests pass cleanly (100% success).

### [2026-08-26] - Identity Model Structure Resolution & Authorization Module Setup
- **Identity Model Consolidation**: Finalized Foreign Key (FK) Link between domain `User` (`StaffUsers`) and ASP.NET Core Identity (`AspNetUsers`). Configured `FK_StaffUsers_AspNetUsers_IdentityUserId` in `UserConfiguration` with cascade delete and index.
- **Entity Cleanup**: Removed redundant `ApplicationUser` entity and configuration.
- **Seeding Enhancement**: Updated `DbSeeder` to automatically seed `Admin`, `FrontDesk`, and `Mechanic` users in both ASP.NET Identity and `StaffUsers` table with role assignments.
- **Authorization Modules Unblocked**: Configured application cookie authentication options, implemented `AccountController` with `LoginViewModel` (Login/Logout/AccessDenied), and enabled role-based authorization (`[Authorize(Roles = ...)]`) for Admin and Mechanic modules.
- **EF Core MS SQL Migration**: Generated fresh initial migration `20260826092543_InitialCreate` targeting MS SQL Server database. Verified all unit/integration tests pass (100% success).

### [2026-08-24] - Final Roadmap Alignment, MS SQL Database Target & MSB1011 Fix
- **Roadmap Alignment**: Adopted `ROADMAP.md` as the official team master plan. Re-aligned all developer vertical slice responsibilities and stage tasks.
- **Database Provider Standard**: Updated primary database configuration to **MS SQL Server** across `appsettings.json` and `DependencyInjection.cs`.
- **MSBuild Error Fix (MSB1011)**: Resolved build failure where MSBuild failed due to multiple solution files (`Garij.sln` and `Garij.slnx`). Updated `.github/workflows/build.yml` and all developer documentation to explicitly specify `Garij.sln` (`dotnet build Garij.sln`, `dotnet test Garij.sln`, `dotnet restore Garij.sln`).
- **Build & Test Verification**: Verified solution compilation and all unit/integration tests pass cleanly.

### [2026-08-24] - Database Schema Definition, EF Core Migrations & Seed Data (Emon)
- Created domain entities `ApplicationUser.cs` and `AiRequestLog.cs`.
- Configured Fluent API database constraints and DbSeeder for roles, admin user, service catalog, and stock parts.

### [2026-08-24] - Architecture Scaffolding & Exception Middleware (Emon)
- Scaffolded solution architecture, implemented global exception middleware, custom exceptions, and test suites.

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

