# Garij — Implementation & File-Level Responsibility Plan

> Source of truth: `Master_Project_Context.md`.
> **Note:** This is a **single ASP.NET Core MVC application with server-rendered Razor Views** — Controllers and Views live in the same app, with no separate client and no separate internal REST API layer between UI and backend (the only genuine external API call is to Gemini). Ownership below follows the module-based plan already agreed — Emon: Foundation/DB/Identity/Job Intake; Rabib: Mechanic-Parts-Billing-Intelligence; Aftab: Customer/Vehicle-Admin-Reporting; Samia: Portal-Notifications-Testing — now broken down to file level.
> No existing repository was provided — **all file paths below are `[PROPOSED FILE]`**, not real files.

---

# 1. Project Technical Overview

Garij is a 3-layer ASP.NET Core MVC application (Presentation/Web → Application/Services → Domain/Infrastructure) built in C#, using EF Core Code-First against SQL Server, ASP.NET Core Identity for auth, and a Google Gemini API integration for advisory intake suggestions. Twelve core entities drive the system, centered on the `ServiceJob` state machine and an atomic completion→invoicing transaction. Four roles (Admin, Front Desk, Mechanic, plus unauthenticated public Customer access) consume role-scoped Razor views backed by MVC controllers and application services.

---

# 2. Complete Work Breakdown

1. Solution/project scaffolding, layered architecture, DI, configuration
2. Domain entities, enums, EF Core `DbContext`, entity configurations, migrations, seed data
3. ASP.NET Core Identity integration, role-based authorization, staff account lifecycle
4. Customer & Vehicle management (CRUD, search, uniqueness)
5. Service Job Intake & Assignment (state machine, booking reference, mechanic assignment)
6. Mechanic diagnostic logging + Parts/Inventory (stock decrement, reorder alerts)
7. Intelligence Engine (Gemini grounding, completion-time estimation, maintenance-due flags)
8. Billing, Invoicing & Payment (atomic transaction, multi-method payments)
9. Public Status Lookup Portal (anonymous, PDF export)
10. Notification & Approval queue
11. Reporting & Analytics (revenue, parts consumption, mechanic workload)
12. Cross-cutting validation/error handling
13. Testing (unit, integration, E2E)
14. Finalization/deployment

---

# 3. Proposed Project/File Structure

**[PROPOSED STRUCTURE]**

```
Garij.sln
src/
├── Garij.Web/                          # ASP.NET Core MVC presentation layer
│   ├── Controllers/
│   ├── Views/
│   │   ├── Shared/
│   │   ├── Account/
│   │   ├── Admin/
│   │   ├── CustomerVehicle/
│   │   ├── ServiceJob/
│   │   ├── Mechanic/
│   │   ├── Billing/
│   │   ├── Notification/
│   │   ├── Reports/
│   │   └── Public/
│   ├── ViewModels/
│   ├── wwwroot/
│   ├── Program.cs
│   └── appsettings.json
│
├── Garij.Application/                  # Business logic / services layer
│   ├── Interfaces/
│   ├── Services/
│   ├── DTOs/
│   └── Validators/
│
├── Garij.Domain/                       # Entities & enums (no dependencies)
│   ├── Entities/
│   └── Enums/
│
└── Garij.Infrastructure/               # EF Core, repositories, external services
    ├── Data/
    │   ├── GarijDbContext.cs
    │   ├── Configurations/
    │   ├── Migrations/
    │   └── SeedData/
    ├── Repositories/
    └── ExternalServices/
        └── Gemini/

tests/
├── Garij.UnitTests/
└── Garij.IntegrationTests/
```

---

# 4. Development Architecture Overview

```
Garij.Web (Controllers + Razor Views)
        │  calls
        ▼
Garij.Application (Services, DTOs, business logic, validation)
        │  calls
        ▼
Garij.Infrastructure (EF Core repositories, GarijDbContext, Gemini client)
        │  persists to
        ▼
SQL Server
```

`Garij.Domain` has no outward dependencies; every other layer references it. This is a standard layered architecture — **[IMPLEMENTATION DECISION]**: exact interface granularity (generic repository vs. per-entity repository) is not specified in the Master Context; a per-feature service + generic repository is proposed below as the simplest fit for the documented feature set.

---

# 5. Feature-by-Feature Implementation Plan

## FEATURE: Identity & Access Management

**Purpose:** Staff login, role-based dashboard routing, staff account lifecycle (FR unspecified numerically but covered by UC-01, NFR-001).
**Users:** Administrator, Front Desk Staff, Mechanic.
**Related Requirements:** FR-026, NFR-001, NFR-007.
**Related Use Cases:** UC-01.
**Related DB Entities:** `User` (ApplicationUser / AspNetUsers — Critical Unknown #2, see below).
**Database Changes:** ASP.NET Core Identity tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`) plus **[IMPLEMENTATION DECISION]** resolving whether the domain `User/Staff` table is the same row as `AspNetUsers` (recommended: extend `IdentityUser` directly as `ApplicationUser` to avoid a duplicate table, consistent with BR-016 role isolation).
**Backend Implementation:** `ApplicationUser : IdentityUser` with `Name`, `Phone`, `Status` fields; `IAuthService`/`AuthService` wrapping `SignInManager`/`UserManager`; role seeding (Admin, FrontDesk, Mechanic); `[Authorize(Roles="...")]` policies per controller.
**UI (Razor) Implementation:** `Views/Account/Login.cshtml`, staff management views under `Views/Admin/Staff/`.
**Files to Create:**
- `[PROPOSED FILE]` `Garij.Domain/Entities/ApplicationUser.cs`
- `[PROPOSED FILE]` `Garij.Domain/Enums/StaffRole.cs`, `StaffStatus.cs`
- `[PROPOSED FILE]` `Garij.Application/Interfaces/IAuthService.cs`
- `[PROPOSED FILE]` `Garij.Application/Services/AuthService.cs`
- `[PROPOSED FILE]` `Garij.Application/DTOs/LoginRequestDto.cs`
- `[PROPOSED FILE]` `Garij.Web/Controllers/AccountController.cs`
- `[PROPOSED FILE]` `Garij.Web/Controllers/AdminController.cs` (staff CRUD)
- `[PROPOSED FILE]` `Garij.Web/Views/Account/Login.cshtml`
- `[PROPOSED FILE]` `Garij.Web/Views/Admin/Staff/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`
- `[PROPOSED FILE]` `Garij.Infrastructure/Data/Configurations/ApplicationUserConfiguration.cs`
**Files to Modify:** `Program.cs` (Identity + role seeding registration).
**Tests:** unit tests for `AuthService` role assignment; integration test for login redirect-by-role (UC-01 alt/exception flows).
**Primary Owner:** Emon.
**Supporting Member:** —
**Dependencies:** Foundation/DbContext must exist first.
**Integration Points:** every controller across all four members' features depends on the `[Authorize(Roles=...)]` policies Emon defines here.
**Acceptance Criteria:** inactive account blocked at login (UC-01 alt flow); wrong role redirected away from disallowed controllers; each role lands on its correct dashboard.

---

## FEATURE: Customer & Vehicle Management

**Purpose:** Register/search customers and vehicles; prevent duplicate plates; view service history.
**Users:** Front Desk Staff (create/search), Administrator (full access).
**Related Requirements:** FR-001–FR-004, BR-001, BR-002, NFR-002.
**Related Use Cases:** UC-02.
**Related DB Entities:** `Customer`, `Vehicle`.
**Database Changes:** `Customer` (CustomerID, Name, Phone, Email, Address), `Vehicle` (VehicleID, CustomerID FK, LicensePlateNumber unique, Make, Model, Year); unique index on `LicensePlateNumber`.
**Backend Implementation:** `ICustomerService`/`CustomerService`, `IVehicleService`/`VehicleService`; plate-format regex + uniqueness validation (NFR-002); service-history query ordered by date (FR-004).
**UI (Razor) Implementation:** search bar with plate lookup, registration form, vehicle history list, all under `Views/CustomerVehicle/`.
**Files to Create:**
- `[PROPOSED FILE]` `Garij.Domain/Entities/Customer.cs`, `Vehicle.cs`
- `[PROPOSED FILE]` `Garij.Application/Interfaces/ICustomerService.cs`, `IVehicleService.cs`
- `[PROPOSED FILE]` `Garij.Application/Services/CustomerService.cs`, `VehicleService.cs`
- `[PROPOSED FILE]` `Garij.Application/DTOs/RegisterCustomerRequestDto.cs`, `VehicleDto.cs`
- `[PROPOSED FILE]` `Garij.Web/Controllers/CustomerVehicleController.cs`
- `[PROPOSED FILE]` `Garij.Web/Views/CustomerVehicle/Search.cshtml`, `Register.cshtml`, `History.cshtml`
- `[PROPOSED FILE]` `Garij.Infrastructure/Data/Configurations/CustomerConfiguration.cs`, `VehicleConfiguration.cs`
- `[PROPOSED FILE]` `Garij.Infrastructure/Repositories/CustomerRepository.cs`, `VehicleRepository.cs`
**Files to Modify:** `GarijDbContext.cs` (DbSets).
**Tests:** duplicate-plate rejection test (UC-02 exception flow); service-history ordering test.
**Primary Owner:** Aftab.
**Supporting Member:** Emon (data model review).
**Dependencies:** Emon's `GarijDbContext` and base repository pattern.
**Integration Points:** Service Job Intake (Emon) consumes `IVehicleService`/`ICustomerService` to select a vehicle when opening a job.
**Acceptance Criteria:** duplicate plate blocked with existing-record link shown; new customer + vehicle persist correctly linked by `CustomerID`.

---

## FEATURE: Service Job Intake & Mechanic Assignment

**Purpose:** Open a job, generate booking reference, assign lead/assistant mechanics, run the status state machine.
**Users:** Front Desk Staff (create/assign), Mechanic (assigned jobs board consumes this data).
**Related Requirements:** FR-005–FR-008, FR-012, FR-013, BR-003, BR-004, BR-005, BR-006.
**Related Use Cases:** UC-03, UC-05.
**Related DB Entities:** `ServiceJob`, `MechanicAssignment`.
**Database Changes:** `ServiceJob` (JobID, VehicleID FK, CustomerID FK, PrimaryMechanicID FK, JobType enum, Status enum, DiagnosticNotes, CreatedAt, CompletedAt); `MechanicAssignment` (AssignmentID, JobID FK, MechanicID FK, AssignedAt, RoleInJob enum). **[CONFLICT — NEEDS DECISION]** noted in Master Context: `ServiceJob.CustomerID` as a direct FK vs. navigating via `Vehicle.CustomerID` — Emon resolves before Stage 2 (recommended: keep the direct FK as specified, since it's explicitly in the ERD, and treat `Vehicle.CustomerID` as the authoritative ownership link with `ServiceJob.CustomerID` as a denormalized convenience field).
**Backend Implementation:** `IServiceJobService`/`ServiceJobService` — booking reference generator (`GRJ-{year}-{sequence}`), lead-mechanic-required validation (BR-003), state machine enforcing BR-005 transitions, completion pre-condition check (BR-006/FR-013) delegating to Rabib's parts/services-logged check before allowing `Completed`.
**UI (Razor) Implementation:** intake form (`Views/ServiceJob/Create.cshtml`) with vehicle picker, job type dropdown, lead mechanic dropdown, and an embedded partial view for Rabib's Gemini suggestion widget; job status board for Front Desk.
**Files to Create:**
- `[PROPOSED FILE]` `Garij.Domain/Entities/ServiceJob.cs`, `MechanicAssignment.cs`
- `[PROPOSED FILE]` `Garij.Domain/Enums/JobType.cs`, `JobStatus.cs`, `RoleInJob.cs`
- `[PROPOSED FILE]` `Garij.Application/Interfaces/IServiceJobService.cs`, `IMechanicAssignmentService.cs`
- `[PROPOSED FILE]` `Garij.Application/Services/ServiceJobService.cs`, `MechanicAssignmentService.cs`, `BookingReferenceGenerator.cs`
- `[PROPOSED FILE]` `Garij.Application/DTOs/CreateServiceJobRequestDto.cs`, `JobStatusDto.cs`
- `[PROPOSED FILE]` `Garij.Web/Controllers/ServiceJobController.cs`
- `[PROPOSED FILE]` `Garij.Web/Views/ServiceJob/Create.cshtml`, `Board.cshtml`, `_GeminiSuggestionPartial.cshtml` (renders Rabib's data)
- `[PROPOSED FILE]` `Garij.Infrastructure/Data/Configurations/ServiceJobConfiguration.cs`, `MechanicAssignmentConfiguration.cs`
**Files to Modify:** `GarijDbContext.cs`.
**Tests:** illegal-transition rejection test (BR-005); mandatory-lead-mechanic test (BR-003); completion-blocked-without-logged-parts test (BR-006, integration test with Rabib's service).
**Primary Owner:** Emon.
**Supporting Member:** Rabib (Gemini suggestion surfacing, completion pre-condition hook).
**Dependencies:** Customer/Vehicle module (Aftab) for vehicle selection; ASP.NET Identity (Emon, self) for mechanic user list.
**Integration Points:** Rabib's Mechanic module reads `ServiceJob`/`MechanicAssignment`; Samia's Notification module listens for status changes; Rabib's Billing module triggers off `Status = Completed`.
**Acceptance Criteria:** job cannot be created without a lead mechanic; status transitions strictly follow BR-005; `Completed` blocked until Rabib's parts/services-logged check passes.

---

## FEATURE: Mechanic Diagnostic Logging & Parts/Inventory

**Purpose:** Record diagnostics, attach catalog services, log parts consumed with price-locking and stock decrement.
**Users:** Mechanic.
**Related Requirements:** FR-009–FR-011, FR-023, BR-008, BR-009, BR-010, NFR-002.
**Related Use Cases:** UC-04.
**Related DB Entities:** `JobServiceDetail`, `Part`, `JobPartUsed`, `ServiceCatalog` (read).
**Database Changes:** `JobServiceDetail` (JobDetailID, JobID FK, ServiceID FK, ServiceCost); `Part` (PartID, PartName, PartNumber unique, UnitPrice, StockQuantity ≥0, ReorderLevel); `JobPartUsed` (JobPartID, JobID FK, PartID FK, QuantityUsed >0, UnitPriceAtTime).
**Backend Implementation:** `IPartsInventoryService`/`PartsInventoryService` — stock-sufficiency check before logging (UC-04 exception flow), atomic decrement, `UnitPriceAtTime` capture at log time (BR-008), reorder-alert trigger when `StockQuantity <= ReorderLevel` (BR-010); `IJobServiceDetailService` for catalog-service attachment.
**UI (Razor) Implementation:** Mechanic Job Board (`Views/Mechanic/Board.cshtml`) restricted to assigned jobs, diagnostic notes modal, parts-logging modal with stock-insufficient warning, color-coded status badges.
**Files to Create:**
- `[PROPOSED FILE]` `Garij.Domain/Entities/JobServiceDetail.cs`, `Part.cs`, `JobPartUsed.cs`
- `[PROPOSED FILE]` `Garij.Application/Interfaces/IPartsInventoryService.cs`, `IJobServiceDetailService.cs`
- `[PROPOSED FILE]` `Garij.Application/Services/PartsInventoryService.cs`, `JobServiceDetailService.cs`
- `[PROPOSED FILE]` `Garij.Application/DTOs/LogPartUsedRequestDto.cs`, `AttachServiceRequestDto.cs`
- `[PROPOSED FILE]` `Garij.Web/Controllers/MechanicController.cs`
- `[PROPOSED FILE]` `Garij.Web/Views/Mechanic/Board.cshtml`, `_DiagnosticModal.cshtml`, `_PartsLogModal.cshtml`
- `[PROPOSED FILE]` `Garij.Infrastructure/Data/Configurations/PartConfiguration.cs`, `JobPartUsedConfiguration.cs`, `JobServiceDetailConfiguration.cs`
**Files to Modify:** `GarijDbContext.cs`; `ServiceJobService.cs` (Emon exposes a `HasLoggedItems(jobId)` hook Rabib's completion-check calls into).
**Tests:** insufficient-stock rejection test (UC-04 exception flow); reorder-alert trigger test (BR-010); price-locking test (a later `Part.UnitPrice` change must not affect an already-logged `JobPartUsed.UnitPriceAtTime`).
**Primary Owner:** Rabib.
**Supporting Member:** Samia (status badge component reused here).
**Dependencies:** Emon's `ServiceJob`/`MechanicAssignment` (job board filters by assigned mechanic).
**Integration Points:** feeds the completion pre-condition Emon's state machine checks; feeds Aftab's Reporting (parts consumption) and Aftab's Admin Dashboard (low-stock table).
**Acceptance Criteria:** stock never goes negative; reorder alert fires at threshold; historical invoices unaffected by later price changes.

---

## FEATURE: Smart Intake Assistant / Intelligence Engine

**Purpose:** Gemini-grounded advisory suggestions, completion-time estimation, maintenance-due flagging — all advisory-only.
**Users:** Front Desk Staff (consumes suggestions), system actor (Intelligence Engine).
**Related Requirements:** FR-021, FR-022, FR-024, FR-025, BR-015.
**Related Use Cases:** UC-03 (step 3, optional).
**Related DB Entities:** none new beyond an audit log table — **[IMPLEMENTATION DECISION]**: add `AiRequestLog` (not in the original ERD, but FR-025 requires "log all AI assistant requests and responses" — flagged since the Master Context's ERD does not include this table explicitly; treat as a necessary supporting table, not a business entity).
**Database Changes:** new `AiRequestLog` table (LogID, RequestText, ResponseText, CreatedAt, StaffUserID) — **[IMPLEMENTATION DECISION]**, confirm with team before migrating.
**Backend Implementation:** `IIntelligenceService`/`IntelligenceService` wrapping a Gemini HTTP client; prompt construction grounded in `ServiceCatalog`; historical-duration query against completed `ServiceJob` records of the same `JobType`; maintenance-due flag query against `Vehicle` service history.
**UI (Razor) Implementation:** the `_GeminiSuggestionPartial.cshtml` rendered inside Emon's intake form (Feature above); staff must click to accept a suggestion (BR-015 — never auto-applied).
**Files to Create:**
- `[PROPOSED FILE]` `Garij.Domain/Entities/AiRequestLog.cs` **[IMPLEMENTATION DECISION]**
- `[PROPOSED FILE]` `Garij.Application/Interfaces/IIntelligenceService.cs`
- `[PROPOSED FILE]` `Garij.Application/Services/IntelligenceService.cs`
- `[PROPOSED FILE]` `Garij.Infrastructure/ExternalServices/Gemini/GeminiClient.cs`, `GeminiPromptBuilder.cs`
- `[PROPOSED FILE]` `Garij.Application/DTOs/IntakeSuggestionResponseDto.cs`
- `[PROPOSED FILE]` `Garij.Web/Views/ServiceJob/_GeminiSuggestionPartial.cshtml` (co-owned with Emon)
**Files to Modify:** `Garij.Web/Views/ServiceJob/Create.cshtml` (Emon embeds Rabib's partial); `appsettings.json` (Gemini API key config).
**Tests:** mock Gemini client test verifying suggestions never auto-write to `JobServiceDetail` without staff action (BR-015); audit log write test.
**Primary Owner:** Rabib.
**Supporting Member:** Emon (hosts the partial in his intake form).
**Dependencies:** `ServiceCatalog` seed data (Emon owns seeding).
**Integration Points:** embedded in Emon's Job Intake screen.
**Acceptance Criteria:** every Gemini call is logged; no suggestion changes a record without explicit staff confirmation click.

---

## FEATURE: Billing, Invoicing & Payment

**Purpose:** Atomically transition a job to Completed and generate its invoice; record multi-method/partial payments.
**Users:** Front Desk Staff.
**Related Requirements:** FR-014–FR-016, BR-006, BR-007, BR-011, BR-012, BR-013, NFR-003.
**Related Use Cases:** UC-05 (Completed transition), UC-06 (invoice + payment).
**Related DB Entities:** `Invoice`, `PaymentTransaction`.
**Database Changes:** `Invoice` (InvoiceID, JobID FK unique, TotalLaborCost, TotalPartsCost, GrandTotal, PaymentStatus enum, CreatedAt); `PaymentTransaction` (TransactionID, InvoiceID FK, AmountPaid, PaymentMethod enum, TransactionDate).
**Backend Implementation:** `IBillingService`/`BillingService` — this is the highest-risk component in the system. Wraps the Completed-transition + `GrandTotal` calculation (`Σ JobServiceDetail.ServiceCost + Σ JobPartUsed.QuantityUsed × UnitPriceAtTime`, BR-011) + `Invoice` insert inside a single EF Core `DbContext.Database.BeginTransaction()` with rollback on any failure (BR-007, NFR-003). `IPaymentService`/`PaymentService` — records `PaymentTransaction`, recalculates `PaymentStatus` (Pending/Paid) per BR-013.
**UI (Razor) Implementation:** billing/checkout screen (`Views/Billing/Checkout.cshtml`) showing itemized labor/parts breakdown, payment-method selector, partial-payment entry.
**Files to Create:**
- `[PROPOSED FILE]` `Garij.Domain/Entities/Invoice.cs`, `PaymentTransaction.cs`
- `[PROPOSED FILE]` `Garij.Domain/Enums/PaymentMethod.cs`, `PaymentStatus.cs`
- `[PROPOSED FILE]` `Garij.Application/Interfaces/IBillingService.cs`, `IPaymentService.cs`
- `[PROPOSED FILE]` `Garij.Application/Services/BillingService.cs`, `PaymentService.cs`
- `[PROPOSED FILE]` `Garij.Application/DTOs/InvoiceDto.cs`, `RecordPaymentRequestDto.cs`
- `[PROPOSED FILE]` `Garij.Web/Controllers/BillingController.cs`
- `[PROPOSED FILE]` `Garij.Web/Views/Billing/Checkout.cshtml`
- `[PROPOSED FILE]` `Garij.Infrastructure/Data/Configurations/InvoiceConfiguration.cs`, `PaymentTransactionConfiguration.cs`
**Files to Modify:** `ServiceJobService.cs` (Emon exposes the `TransitionToCompleted` hook that `BillingService` calls into within the same transaction).
**Tests:** rollback test (simulated failure mid-transaction must leave no partial `Invoice`/status change — NFR-003); `GrandTotal` calculation test (BR-011); 1:1 invoice cardinality test (BR-012); partial-payment-then-full-payment status test (BR-013).
**Primary Owner:** Rabib.
**Supporting Member:** Emon (shared transaction infrastructure pattern — pair review recommended given this is the single highest-risk feature, see Section 24).
**Dependencies:** Emon's `ServiceJob` state machine; Rabib's own parts/services logging (self).
**Integration Points:** Samia's Public Portal reads `Invoice` for customer display/PDF; Aftab's Reporting reads `Invoice`/`PaymentTransaction`; Samia's Notification listens for the Completed trigger this feature causes.
**Acceptance Criteria:** no invoice can be created without a preceding valid Completed transition; a forced failure mid-transaction leaves zero database changes; `GrandTotal` always matches BR-011's formula exactly.

---

## FEATURE: Public Status Lookup Portal

**Purpose:** Unauthenticated status/invoice lookup by plate or booking reference; PDF export.
**Users:** Customer (public, no login).
**Related Requirements:** FR-017, FR-018.
**Related Use Cases:** UC-07.
**Related DB Entities:** `ServiceJob`, `Vehicle`, `Invoice` (read-only).
**Database Changes:** none new; read-only queries against existing entities.
**Backend Implementation:** `IPublicLookupService`/`PublicLookupService` — query by plate or booking reference (no auth); "record not found" friendly message path (UC-07 exception flow); PDF generation service for invoice export — **[IMPLEMENTATION DECISION]**: library choice for PDF generation not specified in Master Context (e.g., QuestPDF or similar) — flagged for team decision.
**UI (Razor) Implementation:** `Views/Public/Lookup.cshtml` (mobile-first, `[AllowAnonymous]`), progress timeline partial, invoice summary partial, PDF download action.
**Files to Create:**
- `[PROPOSED FILE]` `Garij.Application/Interfaces/IPublicLookupService.cs`, `IInvoicePdfService.cs`
- `[PROPOSED FILE]` `Garij.Application/Services/PublicLookupService.cs`, `InvoicePdfService.cs`
- `[PROPOSED FILE]` `Garij.Web/Controllers/PublicPortalController.cs` (`[AllowAnonymous]`)
- `[PROPOSED FILE]` `Garij.Web/Views/Public/Lookup.cshtml`, `_StatusTimelinePartial.cshtml`, `_InvoiceSummaryPartial.cshtml`
**Files to Modify:** none outside this feature (read-only consumer).
**Tests:** not-found friendly-message test (UC-07 exception flow); PDF export produces itemized labor/parts breakdown matching `Invoice`.
**Primary Owner:** Samia.
**Supporting Member:** Rabib (invoice data shape).
**Dependencies:** Emon's `ServiceJob`, Rabib's `Invoice`.
**Integration Points:** read-only consumer of Emon's and Rabib's data — no write-back.
**Acceptance Criteria:** works with zero authentication; correct friendly error on unknown plate/reference; PDF matches on-screen invoice exactly.

---

## FEATURE: Notification & Approval Management

**Purpose:** Auto-generate notifications on status triggers; staff approval queue before dispatch.
**Users:** Administrator, Front Desk Staff (approve), system (auto-generate).
**Related Requirements:** FR-019, FR-020, BR-014.
**Related Use Cases:** UC-08.
**Related DB Entities:** `Notification`.
**Database Changes:** `Notification` (NotificationID, CustomerID FK, JobID FK, Message, IsApproved enum, SentAt nullable).
**Backend Implementation:** `INotificationService`/`NotificationService` — creates a `Pending` notification when `ServiceJob.Status` becomes `Completed` or requires customer approval (event hook into Emon's state machine); approve/reject/dispatch logic (BR-014).
**UI (Razor) Implementation:** `Views/Notification/Queue.cshtml` — approval queue with approve/reject buttons.
**Files to Create:**
- `[PROPOSED FILE]` `Garij.Domain/Entities/Notification.cs`
- `[PROPOSED FILE]` `Garij.Domain/Enums/NotificationApproval.cs`
- `[PROPOSED FILE]` `Garij.Application/Interfaces/INotificationService.cs`
- `[PROPOSED FILE]` `Garij.Application/Services/NotificationService.cs`
- `[PROPOSED FILE]` `Garij.Web/Controllers/NotificationController.cs`
- `[PROPOSED FILE]` `Garij.Web/Views/Notification/Queue.cshtml`
- `[PROPOSED FILE]` `Garij.Infrastructure/Data/Configurations/NotificationConfiguration.cs`
**Files to Modify:** `ServiceJobService.cs` (Emon adds a status-change event/hook Samia subscribes to).
**Tests:** notification auto-creation-on-Completed test; approve/reject state test; no dispatch without approval test (BR-014).
**Primary Owner:** Samia.
**Supporting Member:** Emon (status-change hook), Aftab (queue entry point surfaced on Admin Dashboard).
**Dependencies:** Emon's `ServiceJob` status events; Rabib's Completed-transition trigger.
**Integration Points:** Aftab's Admin Dashboard links into this queue.
**Acceptance Criteria:** a notification is never dispatched without explicit staff approval.

---

## FEATURE: Reporting & Analytics

**Purpose:** Monthly revenue, parts consumption, mechanic workload reports.
**Users:** Administrator.
**Related Requirements:** FR-028.
**Related Use Cases:** UC-09.
**Related DB Entities:** `Invoice`, `PaymentTransaction`, `JobPartUsed`, `ServiceJob`, `User` (all read-only).
**Database Changes:** none new; aggregation queries only.
**Backend Implementation:** `IReportingService`/`ReportingService` — date-ranged aggregation queries for the three report types; print/export formatting — **[IMPLEMENTATION DECISION]**: export format (PDF/CSV) not specified.
**UI (Razor) Implementation:** `Views/Reports/Index.cshtml` (report type + date range selector), `RevenueReport.cshtml`, `PartsConsumptionReport.cshtml`, `MechanicWorkloadReport.cshtml`.
**Files to Create:**
- `[PROPOSED FILE]` `Garij.Application/Interfaces/IReportingService.cs`
- `[PROPOSED FILE]` `Garij.Application/Services/ReportingService.cs`
- `[PROPOSED FILE]` `Garij.Application/DTOs/RevenueReportDto.cs`, `PartsConsumptionReportDto.cs`, `MechanicWorkloadReportDto.cs`
- `[PROPOSED FILE]` `Garij.Web/Controllers/ReportsController.cs`
- `[PROPOSED FILE]` `Garij.Web/Views/Reports/Index.cshtml`, `RevenueReport.cshtml`, `PartsConsumptionReport.cshtml`, `MechanicWorkloadReport.cshtml`
**Files to Modify:** none (read-only).
**Tests:** aggregation-correctness test against seeded data for each of the three report types.
**Primary Owner:** Aftab.
**Supporting Member:** Rabib (source data shape for Invoice/PaymentTransaction/JobPartUsed).
**Dependencies:** Rabib's Billing/Parts data must exist and be populated before reports are meaningful.
**Integration Points:** consumes Rabib's data read-only; surfaced from Aftab's Admin Dashboard.
**Acceptance Criteria:** report totals reconcile exactly against underlying `Invoice`/`PaymentTransaction`/`JobPartUsed` records for the selected range.

---

# 6. Database Implementation Plan

| Entity | Owner (schema) | Key Fields | FKs | Constraints | Migration Owner | Related Services | Related UI | Test Owner |
|---|---|---|---|---|---|---|---|---|
| `Customer` | Emon | Name, Phone, Email, Address | — | required Name/Phone | Emon | CustomerService (Aftab) | CustomerVehicle views (Aftab) | Aftab |
| `Vehicle` | Emon | LicensePlateNumber (unique), Make, Model, Year | CustomerID | plate unique + regex | Emon | VehicleService (Aftab) | CustomerVehicle views (Aftab) | Aftab |
| `ApplicationUser` | Emon | Name, Email(unique), Role, Phone, Status | — | Identity constraints | Emon | AuthService (Emon) | Account/Admin views (Emon) | Emon |
| `ServiceCatalog` | Emon | ServiceName, BaseCost, Description | — | seeded, admin-editable | Emon | (read by Rabib/Emon) | Admin catalog mgmt (Aftab) | Emon |
| `ServiceJob` | Emon | JobType, Status, DiagnosticNotes, CreatedAt, CompletedAt | VehicleID, CustomerID, PrimaryMechanicID | mandatory lead mechanic, valid transitions | Emon | ServiceJobService (Emon) | Intake/Board views (Emon) | Emon |
| `JobServiceDetail` | Emon | ServiceCost | JobID, ServiceID | — | Emon | JobServiceDetailService (Rabib) | Mechanic board (Rabib) | Rabib |
| `MechanicAssignment` | Emon | AssignedAt, RoleInJob | JobID, MechanicID | non-lead for extras | Emon | MechanicAssignmentService (Emon) | Intake views (Emon) | Emon |
| `Part` | Emon | PartName, PartNumber(unique), UnitPrice, StockQuantity(≥0), ReorderLevel | — | non-negative stock | Emon | PartsInventoryService (Rabib) | Mechanic board / Admin catalog (Rabib/Aftab) | Rabib |
| `JobPartUsed` | Emon | QuantityUsed(>0), UnitPriceAtTime | JobID, PartID | price locked at insert | Emon | PartsInventoryService (Rabib) | Mechanic board (Rabib) | Rabib |
| `Invoice` | Emon | TotalLaborCost, TotalPartsCost, GrandTotal, PaymentStatus | JobID (unique) | 1:1 with ServiceJob | Emon | BillingService (Rabib) | Billing/Portal views (Rabib/Samia) | Rabib |
| `PaymentTransaction` | Emon | AmountPaid, PaymentMethod, TransactionDate | InvoiceID | — | Emon | PaymentService (Rabib) | Billing views (Rabib) | Rabib |
| `Notification` | Emon | Message, IsApproved, SentAt | CustomerID, JobID | — | Emon | NotificationService (Samia) | Notification queue (Samia) | Samia |
| `AiRequestLog` **[IMPLEMENTATION DECISION]** | Emon | RequestText, ResponseText, CreatedAt | StaffUserID | — | Emon | IntelligenceService (Rabib) | Intake partial (Rabib) | Rabib |

**DbContext, entity configurations, migrations, and seed data are centrally owned by Emon** even though each entity's business logic is owned by different members — this avoids migration conflicts (see Section 13's Handoff Contracts).

---

# 7. Backend Implementation Plan

| Feature | Controller | Service | Repository | DTOs | Validation | Business Logic Owner |
|---|---|---|---|---|---|---|
| Identity | `AccountController`, `AdminController` (staff) | `AuthService` | (Identity built-in) | `LoginRequestDto` | credential/role checks | Emon |
| Customer/Vehicle | `CustomerVehicleController` | `CustomerService`, `VehicleService` | `CustomerRepository`, `VehicleRepository` | `RegisterCustomerRequestDto`, `VehicleDto` | plate regex/uniqueness (NFR-002) | Aftab |
| Service Job Intake | `ServiceJobController` | `ServiceJobService`, `MechanicAssignmentService`, `BookingReferenceGenerator` | `ServiceJobRepository` | `CreateServiceJobRequestDto`, `JobStatusDto` | lead-mechanic-required, transition validity | Emon |
| Mechanic/Parts | `MechanicController` | `PartsInventoryService`, `JobServiceDetailService` | `PartRepository`, `JobPartUsedRepository` | `LogPartUsedRequestDto` | stock sufficiency, non-negative | Rabib |
| Intelligence | (embedded in ServiceJobController via partial + AJAX action) | `IntelligenceService`, `GeminiClient` | `AiRequestLogRepository` | `IntakeSuggestionResponseDto` | advisory-only gate (BR-015) | Rabib |
| Billing | `BillingController` | `BillingService`, `PaymentService` | `InvoiceRepository`, `PaymentTransactionRepository` | `InvoiceDto`, `RecordPaymentRequestDto` | GrandTotal formula, atomic transaction | Rabib |
| Public Portal | `PublicPortalController` | `PublicLookupService`, `InvoicePdfService` | (reads existing repos) | — | not-found handling | Samia |
| Notification | `NotificationController` | `NotificationService` | `NotificationRepository` | — | approval gate | Samia |
| Reporting | `ReportsController` | `ReportingService` | (reads existing repos) | `RevenueReportDto`, etc. | date-range validation | Aftab |

Error handling convention (**[IMPLEMENTATION DECISION]**, not specified in Master Context): Emon defines a shared exception-handling middleware/filter pattern in `Garij.Web`; each service owner throws typed exceptions (`ValidationException`, `BusinessRuleException`) their own controller catches per the shared convention.

---

# 8. API Implementation Plan (MVC Actions + External Gemini API)

Since this is a single server-rendered MVC app, controller actions call application services directly — there's no internal REST API layer between UI and backend. The only genuine external API is Gemini.

| Endpoint (external) | Direction | Purpose | Owner |
|---|---|---|---|
| Google Gemini API | Outbound call | Grounded intake category suggestion | Rabib |

| MVC Action (internal, not a public API) | Route (indicative) | Purpose | Owner |
|---|---|---|---|
| `AccountController.Login` | `POST /Account/Login` | Authenticate staff | Emon |
| `CustomerVehicleController.Search` | `GET /CustomerVehicle/Search` | Plate lookup | Aftab |
| `ServiceJobController.Create` | `POST /ServiceJob/Create` | Open job | Emon |
| `ServiceJobController.GetSuggestion` (AJAX) | `POST /ServiceJob/GetSuggestion` | Gemini suggestion | Rabib |
| `MechanicController.LogPart` | `POST /Mechanic/LogPart` | Log part used | Rabib |
| `BillingController.Complete` | `POST /Billing/Complete` | Atomic completion+invoice | Rabib (calls Emon's `TransitionToCompleted` hook) |
| `BillingController.RecordPayment` | `POST /Billing/RecordPayment` | Record payment | Rabib |
| `PublicPortalController.Lookup` | `GET /Public/Lookup` | Anonymous status lookup | Samia |
| `NotificationController.Approve` | `POST /Notification/Approve` | Approve/dispatch | Samia |
| `ReportsController.Revenue` (etc.) | `GET /Reports/Revenue` | Aggregated report | Aftab |

---

# 9. UI (Razor Views) Implementation Plan

## Login Screen
**Purpose:** Staff authentication. **Role:** All staff. **Controls:** email/username, password, submit. **API/Backend Call:** `AccountController.Login` → `AuthService`. **Validation:** required fields, inactive-account message. **Navigation:** redirects by role post-login. **Files:** `[PROPOSED FILE]` `Views/Account/Login.cshtml`. **Owner:** Emon.

## Admin Dashboard
**Purpose:** KPI cards, low-stock table, notification queue entry, staff mgmt links, report links. **Role:** Admin. **Backend Dependency:** `PartsInventoryService` (Rabib, low-stock), `NotificationService` (Samia, queue count), `ReportingService` (Aftab, self). **Files:** `[PROPOSED FILE]` `Views/Admin/Dashboard.cshtml`. **Owner:** Aftab. **Supporting:** Rabib, Samia (data feeds).

## Customer/Vehicle Search & Registration
**Purpose:** Search/register customers+vehicles. **Role:** Front Desk. **Backend Dependency:** `CustomerService`, `VehicleService` (self). **Files:** `[PROPOSED FILE]` `Views/CustomerVehicle/Search.cshtml`, `Register.cshtml`. **Owner:** Aftab.

## Service Job Intake Form
**Purpose:** Open job, select type, assign lead mechanic, view Gemini suggestion. **Role:** Front Desk. **Backend Dependency:** `ServiceJobService` (self), `IntelligenceService` (Rabib, embedded partial). **Files:** `[PROPOSED FILE]` `Views/ServiceJob/Create.cshtml`. **Owner:** Emon. **Supporting:** Rabib (partial).

## Mechanic Job Board
**Purpose:** Touch-friendly assigned-jobs list, diagnostic notes modal, parts-logging modal. **Role:** Mechanic. **Backend Dependency:** `ServiceJobService` (Emon, job list), `PartsInventoryService`/`JobServiceDetailService` (self). **Files:** `[PROPOSED FILE]` `Views/Mechanic/Board.cshtml`, `_DiagnosticModal.cshtml`, `_PartsLogModal.cshtml`. **Owner:** Rabib.

## Billing / Checkout Screen
**Purpose:** Trigger Completed transition, show itemized invoice, record payment. **Role:** Front Desk. **Backend Dependency:** `BillingService`, `PaymentService` (self). **Files:** `[PROPOSED FILE]` `Views/Billing/Checkout.cshtml`. **Owner:** Rabib.

## Public Status Lookup Portal
**Purpose:** Anonymous plate/reference lookup, status timeline, invoice + PDF. **Role:** Customer (public). **Backend Dependency:** `PublicLookupService`, `InvoicePdfService` (self). **Files:** `[PROPOSED FILE]` `Views/Public/Lookup.cshtml`. **Owner:** Samia.

## Notification Approval Queue
**Purpose:** Approve/reject pending notifications. **Role:** Admin/Front Desk. **Backend Dependency:** `NotificationService` (self). **Files:** `[PROPOSED FILE]` `Views/Notification/Queue.cshtml`. **Owner:** Samia.

## Reports Screens
**Purpose:** Filter/view/print the three report types. **Role:** Admin. **Backend Dependency:** `ReportingService` (self, reads Rabib's data). **Files:** `[PROPOSED FILE]` `Views/Reports/*.cshtml`. **Owner:** Aftab.

---

# 10. Authentication & Authorization Plan

```
Database (AspNetUsers, AspNetRoles)             — Emon
        ↓
ApplicationUser entity                           — Emon
        ↓
AuthService (SignInManager/UserManager wrapper)  — Emon
        ↓
AccountController.Login                          — Emon
        ↓
[Authorize(Roles="...")] on every controller      — Emon defines; each owner applies to their own controller
        ↓
Role-based dashboard redirect                     — Emon (login redirect logic); Aftab (Admin dashboard), Rabib (Mechanic board), Emon (Front Desk landing)
        ↓
PublicPortalController [AllowAnonymous]           — Samia (explicit opt-out of auth)
```

Each feature owner is responsible for applying the correct `[Authorize(Roles=...)]` attribute to their own controller per the Roles & Permissions Matrix in the Master Context (e.g., Rabib must ensure `MechanicController` and `BillingController` enforce BR-016 role isolation — mechanics never get billing access).

---

# 11. File/Component Responsibility Matrix

*(All paths `[PROPOSED FILE]` — no existing repository.)*

| File/Component | Purpose | Action | Owner | Support | Feature |
|---|---|---|---|---|---|
| `Garij.Domain/Entities/ApplicationUser.cs` | Identity user | Create | Emon | — | Identity |
| `Garij.Application/Services/AuthService.cs` | Auth logic | Create | Emon | — | Identity |
| `Garij.Web/Controllers/AccountController.cs` | Login | Create | Emon | — | Identity |
| `Garij.Web/Views/Account/Login.cshtml` | Login UI | Create | Emon | — | Identity |
| `Garij.Domain/Entities/Customer.cs` | Entity | Create | Emon (schema) | Aftab (logic) | Customer/Vehicle |
| `Garij.Domain/Entities/Vehicle.cs` | Entity | Create | Emon (schema) | Aftab (logic) | Customer/Vehicle |
| `Garij.Application/Services/CustomerService.cs` | Business logic | Create | Aftab | Emon | Customer/Vehicle |
| `Garij.Web/Controllers/CustomerVehicleController.cs` | Controller | Create | Aftab | — | Customer/Vehicle |
| `Garij.Web/Views/CustomerVehicle/Search.cshtml` | UI | Create | Aftab | — | Customer/Vehicle |
| `Garij.Domain/Entities/ServiceJob.cs` | Entity | Create | Emon (schema) | Emon (logic) | Job Intake |
| `Garij.Application/Services/ServiceJobService.cs` | State machine | Create | Emon | Rabib (completion hook) | Job Intake |
| `Garij.Web/Controllers/ServiceJobController.cs` | Controller | Create | Emon | Rabib | Job Intake |
| `Garij.Web/Views/ServiceJob/Create.cshtml` | Intake UI | Create | Emon | Rabib (partial) | Job Intake |
| `Garij.Application/Services/PartsInventoryService.cs` | Inventory logic | Create | Rabib | — | Mechanic/Parts |
| `Garij.Web/Controllers/MechanicController.cs` | Controller | Create | Rabib | — | Mechanic/Parts |
| `Garij.Web/Views/Mechanic/Board.cshtml` | UI | Create | Rabib | — | Mechanic/Parts |
| `Garij.Application/Services/IntelligenceService.cs` | Gemini logic | Create | Rabib | Emon (host UI) | Intelligence |
| `Garij.Infrastructure/ExternalServices/Gemini/GeminiClient.cs` | External API client | Create | Rabib | — | Intelligence |
| `Garij.Application/Services/BillingService.cs` | Atomic billing | Create | Rabib | Emon (pair review) | Billing |
| `Garij.Web/Controllers/BillingController.cs` | Controller | Create | Rabib | — | Billing |
| `Garij.Web/Views/Billing/Checkout.cshtml` | UI | Create | Rabib | — | Billing |
| `Garij.Application/Services/PublicLookupService.cs` | Public query logic | Create | Samia | — | Public Portal |
| `Garij.Web/Controllers/PublicPortalController.cs` | Controller | Create | Samia | — | Public Portal |
| `Garij.Web/Views/Public/Lookup.cshtml` | UI | Create | Samia | — | Public Portal |
| `Garij.Application/Services/NotificationService.cs` | Notification logic | Create | Samia | Emon (status hook) | Notification |
| `Garij.Web/Controllers/NotificationController.cs` | Controller | Create | Samia | — | Notification |
| `Garij.Web/Views/Notification/Queue.cshtml` | UI | Create | Samia | — | Notification |
| `Garij.Application/Services/ReportingService.cs` | Aggregation logic | Create | Aftab | Rabib (data shape) | Reporting |
| `Garij.Web/Controllers/ReportsController.cs` | Controller | Create | Aftab | — | Reporting |
| `Garij.Web/Views/Reports/*.cshtml` | UI | Create | Aftab | — | Reporting |
| `Garij.Infrastructure/Data/GarijDbContext.cs` | EF Core context | Create/Modify (all features add DbSets) | Emon | All (request changes via Emon) | Foundation |
| `Garij.Infrastructure/Data/Migrations/*` | Schema migrations | Create | Emon | — | Foundation |
| `Garij.Infrastructure/Data/SeedData/DbSeeder.cs` | Seed ServiceCatalog/Part | Create | Emon | — | Foundation |
| `Program.cs` | App startup, DI registration | Modify (all features register their services) | Emon | All | Foundation |

---

# 12. Feature Ownership Matrix

| Feature | Database | Backend | UI | Testing | Primary Owner |
|---|---|---|---|---|---|
| Identity & Access | Emon | Emon | Emon | Emon | Emon |
| Customer & Vehicle | Emon (schema) | Aftab | Aftab | Aftab | Aftab |
| Service Job Intake & Assignment | Emon | Emon | Emon | Emon | Emon |
| Mechanic Diagnostics & Parts/Inventory | Emon (schema) | Rabib | Rabib | Rabib | Rabib |
| Intelligence Engine (Gemini) | Emon (schema) | Rabib | Rabib (+Emon host) | Rabib | Rabib |
| Billing, Invoicing & Payment | Emon (schema) | Rabib | Rabib | Rabib | Rabib |
| Public Status Lookup Portal | — (read-only) | Samia | Samia | Samia | Samia |
| Notification & Approval | Emon (schema) | Samia | Samia | Samia | Samia |
| Reporting & Analytics | — (read-only) | Aftab | Aftab | Aftab | Aftab |

---

# 13. Cross-Member Dependencies

| Component | Owner | Depends On | Supporting Member |
|---|---|---|---|
| `AuthService` | Emon | `ApplicationUser`, `GarijDbContext` | — |
| `CustomerService`/`VehicleService` | Aftab | `GarijDbContext`, entity schema | Emon |
| `ServiceJobService` (state machine) | Emon | `MechanicAssignment`, Identity roles | — |
| Job Intake Form | Emon | `CustomerService`/`VehicleService`, `IntelligenceService` | Aftab, Rabib |
| `PartsInventoryService` | Rabib | `ServiceJobService` (job context), `Part`/`JobPartUsed` schema | Emon |
| `IntelligenceService` | Rabib | `ServiceCatalog` seed data | Emon |
| `BillingService` | Rabib | `ServiceJobService.TransitionToCompleted` hook, `PartsInventoryService` completion check | Emon |
| Mechanic Job Board | Rabib | `ServiceJobService` (assigned-jobs query) | Emon |
| `NotificationService` | Samia | `ServiceJobService` status-change events, `BillingService` Completed trigger | Emon, Rabib |
| `PublicLookupService` | Samia | `ServiceJobService`, `BillingService`'s `Invoice` | Emon, Rabib |
| `ReportingService` | Aftab | `Invoice`, `PaymentTransaction`, `JobPartUsed` (Rabib's data) | Rabib |
| Admin Dashboard | Aftab | `PartsInventoryService` (low-stock), `NotificationService` (queue count) | Rabib, Samia |

---

# 14. Handoff Contracts

### Database → Backend (Emon → everyone)
Emon provides: finalized entity classes, EF Core configuration, applied migration, and a short note on any constraint (e.g., unique index) other owners' services must respect. Requesting owner must not write their own migration against the same table.

### Backend → UI (self-handoff within each feature, since owners are end-to-end)
Each feature owner provides, for their own use: service method signature, DTO shape, and thrown exception types, before starting the corresponding Razor view — this is now an internal contract rather than a cross-person one, since ownership is end-to-end.

### Cross-feature service → consuming feature (e.g., Rabib's `BillingService` → Samia's `PublicLookupService`)
Provider must supply: the DTO/entity shape returned, nullability guarantees, and whether the query is safe for anonymous/read-only use. Rabib confirms `Invoice` fields are stable before Samia builds the Portal's invoice display.

### Feature Developer → Tester (Samia, for E2E coordination)
Each owner provides: a working feature build, seed/test data needed to exercise it, expected behavior per Acceptance Criteria (Section 5), and known limitations/`[IMPLEMENTATION DECISION]` items still open.

---

# 15. Parallel Development Plan

```
                         Emon: Database + Identity + Job Intake
                                        │
                    ┌───────────────────┼───────────────────┐
                    ▼                   ▼                   ▼
         Rabib: Mechanic/Parts    Aftab: Customer/Vehicle   Samia: (scaffolding only
         + Billing + Intelligence  + Admin + Reporting       until job/billing data exists)
                    │                   │                   │
                    └─────────┬─────────┴─────────┬─────────┘
                               ▼                   ▼
                        Aftab's Reporting    Samia's Portal + Notifications
                         (needs Rabib's         (needs Emon's job data +
                          billing data)          Rabib's invoice data)
                                        │
                                        ▼
                                  Integration + Testing
```

Boundary: nothing meaningful can be built by Rabib, Aftab, or Samia until Emon's schema + Identity are stable. Aftab's Customer/Vehicle module can proceed almost immediately after that (few dependencies). Samia's Portal and Notification work are the most downstream — they need both Emon's job data and Rabib's billing data to be functionally complete, so Samia should scaffold UI/skeleton early but defer full wiring to Stage 3.

---

# 16. Development Sequence

| Stage | What | Who | Files/Components | Must Precede | Can Be Parallel |
|---|---|---|---|---|---|
| 1. Repo/solution setup | `.sln`, project references, folder structure | Emon | `Garij.sln`, all `.csproj` | everything | — |
| 2. Configuration | `appsettings.json`, DI container skeleton | Emon | `Program.cs` | Stage 3+ | — |
| 3. Database & entities | 12 entities, `GarijDbContext`, configurations, initial migration, seed | Emon | `Garij.Domain/Entities/*`, `GarijDbContext.cs` | everything downstream | Rabib/Aftab/Samia can scaffold service *interfaces* against a draft schema |
| 4. Identity | `ApplicationUser`, roles, `AuthService`, login | Emon | `AccountController.cs`, `Login.cshtml` | all `[Authorize]` controllers | — |
| 5. Job Intake & state machine | `ServiceJobService`, intake form | Emon | `ServiceJobController.cs`, `Create.cshtml` | Billing, Mechanic board, Notifications | Aftab's Customer/Vehicle module |
| 6. Customer/Vehicle | full module | Aftab | `CustomerVehicleController.cs` etc. | Job Intake's vehicle picker | parallel with Stage 5 |
| 7. Mechanic/Parts + Intelligence | full modules | Rabib | `MechanicController.cs`, `IntelligenceService.cs` | Billing's completion check | parallel with Stage 6 |
| 8. Billing | atomic transaction, checkout UI | Rabib | `BillingService.cs`, `Checkout.cshtml` | Portal, Reporting, Notifications | after Stage 7 |
| 9. Public Portal | full module | Samia | `PublicPortalController.cs` etc. | — | after Stage 8 (needs invoice data) |
| 10. Notifications | full module | Samia | `NotificationController.cs` etc. | — | parallel with Stage 9 |
| 11. Admin Dashboard & Reporting | full modules | Aftab | `AdminController.cs`, `ReportsController.cs` | — | after Stage 8 (needs billing data) |
| 12. Integration | wire remaining cross-feature dependencies | All, Emon coordinates | — | — | — |
| 13. Testing | unit/integration/E2E | All, Samia coordinates E2E | `tests/*` | — | ongoing from Stage 4 onward per-feature |
| 14. Bug fixing / finalization | — | All, Emon coordinates | — | — | — |

---

# 17. Emon — Detailed Responsibility

**Main Responsibilities:** Architecture, database, Identity, Service Job Intake & Assignment (full stack).
**Modules Owned:** Foundation, Database, Identity & Access, Service Job Intake & Assignment.
**Features Owned:** Identity & Access Management; Service Job Intake & Mechanic Assignment.
**Database Components:** all 12 entity classes, `GarijDbContext`, all EF Core configurations, all migrations, seed data.
**Backend Components:** `AuthService`, `ServiceJobService`, `MechanicAssignmentService`, `BookingReferenceGenerator`.
**API Components:** `AccountController`, `ServiceJobController` (MVC actions).
**Business Logic:** ServiceJob state machine (BR-005), lead-mechanic requirement (BR-003), completion pre-condition orchestration (calls Rabib's check).
**Authentication/Authorization:** full ownership — role seeding, `[Authorize]` policy definitions others apply.
**Files to Create:** listed in Sections 5 and 11 under Identity and Job Intake features.
**Files to Modify:** `Program.cs`, `GarijDbContext.cs` (as other features add DbSets).
**Supporting Responsibilities:** database schema review for Aftab's Customer/Vehicle module; hosting Rabib's Gemini suggestion partial; exposing status-change hooks for Samia's Notifications; exposing `TransitionToCompleted` hook for Rabib's Billing; pair-reviewing Rabib's atomic transaction.
**Dependencies on Rabib:** completion pre-condition check (parts/services logged) before allowing `Completed`.
**Dependencies on Aftab:** none blocking (Customer/Vehicle picker consumed, not depended on structurally).
**Dependencies on Samia:** none.
**Expected Deliverables:** working solution skeleton, complete schema+migrations, functioning login/roles, fully working Job Intake screen with enforced state machine.
**Estimated Workload:** ~30%.

---

# 18. Rabib — Detailed Responsibility

**Main Responsibilities:** Mechanic execution & parts/inventory, atomic billing engine, Gemini-powered Intelligence Engine.
**Modules Owned:** Mechanic Diagnostics & Parts/Inventory, Billing/Invoicing/Payment, Intelligence Engine.
**Features Owned:** Mechanic Diagnostic Logging & Parts/Inventory; Smart Intake Assistant/Intelligence Engine; Billing, Invoicing & Payment.
**Database Components:** business logic against `Part`, `JobPartUsed`, `JobServiceDetail`, `Invoice`, `PaymentTransaction`, `AiRequestLog` (schema owned by Emon; logic owned by Rabib).
**Backend Components:** `PartsInventoryService`, `JobServiceDetailService`, `IntelligenceService`, `GeminiClient`, `BillingService`, `PaymentService`.
**API Components:** `MechanicController`, `BillingController` (MVC actions); Gemini external API integration.
**Business Logic:** stock decrement/reorder alert (BR-009, BR-010), price-locking (BR-008), the atomic Completed+invoice transaction (BR-006, BR-007, BR-011, BR-012), payment settlement (BR-013), advisory-only AI gating (BR-015).
**Authentication/Authorization:** applies `[Authorize(Roles="Mechanic")]`/`[Authorize(Roles="FrontDesk")]` appropriately per BR-016 role isolation on his own controllers.
**Files to Create:** listed in Sections 5 and 11 under Mechanic/Parts, Intelligence, and Billing features.
**Files to Modify:** `GarijDbContext.cs` (DbSet additions coordinated through Emon), `ServiceJobService.cs` integration point (coordinated with Emon).
**Supporting Responsibilities:** provides `Invoice` data shape to Samia's Portal and Aftab's Reporting.
**Dependencies on Emon:** `ServiceJob`/`MechanicAssignment` schema and state machine hooks.
**Dependencies on Aftab:** none blocking.
**Dependencies on Samia:** none blocking.
**Expected Deliverables:** working Mechanic Job Board end-to-end, atomic billing engine with checkout UI (critical-path deliverable), functioning Gemini-grounded advisory suggestions.
**Estimated Workload:** ~27%.

---

# 19. Aftab — Detailed Responsibility

**Main Responsibilities:** Customer & Vehicle management, Admin Dashboard, Reporting & Analytics.
**Modules Owned:** Customer & Vehicle Management, Admin Dashboard, Reporting & Analytics.
**Features Owned:** Customer & Vehicle Management; Reporting & Analytics (Admin Dashboard is a composite view drawing on his own + others' data).
**Database Components:** business logic against `Customer`, `Vehicle` (schema owned by Emon).
**Backend Components:** `CustomerService`, `VehicleService`, `ReportingService`.
**API Components:** `CustomerVehicleController`, `AdminController`, `ReportsController` (MVC actions).
**Business Logic:** plate-uniqueness validation (BR-002), customer-vehicle binding (BR-001), report aggregation.
**Authentication/Authorization:** applies `[Authorize(Roles="Admin")]`/`[Authorize(Roles="FrontDesk")]` appropriately on his own controllers.
**Files to Create/Modify:** listed in Sections 5 and 11 under Customer/Vehicle and Reporting features.
**Supporting Responsibilities:** none blocking others — his modules are consumed read-only by Emon's Job Intake (vehicle picker) and nobody else structurally depends on his output to build their own feature.
**Dependencies on Emon:** database schema, Identity roles.
**Dependencies on Rabib:** Reporting requires Rabib's `Invoice`/`PaymentTransaction`/`JobPartUsed` data to exist and be populated before it's meaningful to test against.
**Dependencies on Samia:** none.
**Expected Deliverables:** functioning Customer/Vehicle module end-to-end, Admin Dashboard, working reporting screens with real aggregated data.
**Estimated Workload:** ~22%.

---

# 20. Samia — Detailed Responsibility

**Main Responsibilities:** Public Status Lookup Portal, Notification & Approval Management, test coordination.
**Modules Owned:** Public Status Lookup Portal, Notification & Approval Management.
**Features Owned:** Public Status Lookup Portal; Notification & Approval Management.
**Database Components:** business logic against `Notification` (schema owned by Emon); read-only queries against `ServiceJob`, `Vehicle`, `Invoice`.
**Backend Components:** `PublicLookupService`, `InvoicePdfService`, `NotificationService`.
**API Components:** `PublicPortalController` (`[AllowAnonymous]`), `NotificationController` (MVC actions).
**Business Logic:** friendly not-found handling (UC-07), notification-approval gate (BR-014).
**Authentication/Authorization:** explicitly opts the Portal *out* of authentication (`[AllowAnonymous]`); applies `[Authorize(Roles="Admin,FrontDesk")]` on the Notification queue.
**Files to Create/Modify:** listed in Sections 5 and 11 under Public Portal and Notification features.
**Supporting Responsibilities:** defines and documents the shared status-badge/color-coding component used across Emon's, Rabib's, and Aftab's screens; coordinates and writes E2E test scenarios; coordinates final system testing across the team.
**Dependencies on Emon:** `ServiceJob` status data and status-change event hook.
**Dependencies on Rabib:** `Invoice` data for the Portal display and PDF export; the Completed-transition trigger for auto-notification.
**Dependencies on Aftab:** none blocking.
**Expected Deliverables:** functioning Public Portal with PDF export, working Notification approval queue, consistent status-badge system, E2E test suite covering primary user journeys.
**Estimated Workload:** ~21%.

---

# 21. Testing Responsibility

- **Developer testing:** each owner unit-tests their own services against the Acceptance Criteria listed per feature in Section 5.
- **Integration testing:** Emon coordinates, with special focus on the Emon↔Rabib boundary (state machine ↔ atomic billing transaction — the highest-risk integration point in the system, per NFR-003).
- **System testing:** all four members; Samia coordinates.
- **Final testing:** all four members participate before finalization; Samia is the named coordinator.
- **Per-feature test emphasis:** Identity — role redirect correctness; Customer/Vehicle — duplicate-plate rejection; Job Intake — illegal transition rejection; Mechanic/Parts — stock non-negativity and price-locking; Billing — transactional rollback and GrandTotal accuracy; Portal — anonymous access and PDF fidelity; Notifications — no-dispatch-without-approval; Reporting — aggregation correctness.

---

# 22. Integration Strategy

- Integrate incrementally as each Stage in Section 16 completes, not in one big-bang merge at the end.
- Database changes always flow through Emon; no other member migrates independently.
- Backend-to-UI is a self-integration concern per feature owner (since ownership is end-to-end), reducing handoff overhead versus the earlier UI/backend split.
- Cross-feature integration points (Job Intake↔Billing, Billing↔Portal, Job Intake↔Notifications, Billing↔Reporting) are the actual coordination risk — Emon and Rabib should sync directly on the state-machine↔billing boundary before Stage 8; Samia and Rabib sync on invoice data shape before Stage 9; Aftab and Rabib sync on reporting data shape before Stage 11.
- Full end-to-end system testing happens only after Stage 11 (all modules functionally complete).
- Emon owns final build verification and deployment prep.

---

# 23. Workload Calculation

| Developer | Responsibilities | Effort | Workload |
|---|---|---:|---:|
| Emon | Foundation (M), Database (L), Identity full-stack (M), Job Intake/State Machine full-stack (L) | M+L+M+L | **~30%** |
| Rabib | Mechanic/Parts full-stack (M), Billing atomic engine full-stack (L), Intelligence Engine (M) | M+L+M | **~27%** |
| Aftab | Customer/Vehicle full-stack (M), Admin Dashboard (M), Reporting full-stack (M) | M+M+M | **~22%** |
| Samia | Public Portal full-stack (M), Notification full-stack (M), Testing coordination (S) | M+M+S | **~21%** |
| **Total** | | | **100%** |

**Emon + Rabib = 57%** ✅ (target 55–60%)
**Aftab + Samia = 43%** ✅ (target 40–45%)

---

# 24. Risk Areas

| Risk | Why It May Happen | Responsible Person | Prevention Strategy |
|---|---|---|---|
| Schema instability blocking everyone | Emon's entities/migrations change after others start building against them | Emon | Freeze schema (including resolving the two Critical Unknowns) before Stage 5 begins; communicate any post-freeze change immediately |
| Atomic billing transaction bugs | Highest-complexity single feature (BR-007); only Rabib deeply understands it | Rabib | Emon pair-reviews this specific component; add a dedicated rollback test before merging |
| State-machine ↔ billing boundary mismatch | Emon's `TransitionToCompleted` hook and Rabib's `BillingService` must agree on transaction scope | Emon & Rabib | Agree on the exact hook signature and transaction boundary in writing before Stage 8 |
| Reporting blocked late in the project | Aftab's Reporting depends entirely on Rabib's billing/parts data existing and being populated | Aftab & Rabib | Rabib exposes a stable data-access shape early (even before full Billing UI is done) so Aftab can build against it |
| Portal/Notifications blocked late | Samia's two features both depend on Emon's job data and Rabib's invoice data | Samia | Samia scaffolds UI against stubbed/mock data in Stage 1–2 rather than waiting for Stage 8 |
| Migration conflicts | Multiple members touching `GarijDbContext.cs`/entity files simultaneously | Emon | All schema changes routed through Emon; no parallel migrations |
| Undefined AI audit table | `AiRequestLog` (FR-025) isn't in the original ERD — flagged `[IMPLEMENTATION DECISION]` | Rabib & Emon | Confirm and add to schema before Stage 3 migration, not after |
| Ambiguous error-handling convention | Not specified in Master Context | Emon | Emon defines the shared exception/middleware pattern in Stage 2, before feature work starts |

---

# 25. Final Responsibility Summary

**Emon →** Architecture, full database & migrations, Identity/Access (full stack), Service Job Intake & Assignment (full stack, including state machine). Integration coordination, deployment finalization.

**Rabib →** Mechanic Diagnostics & Parts/Inventory (full stack), Billing/Invoicing/Payment (full stack, including the atomic transaction), Intelligence Engine/Gemini integration (full stack).

**Aftab →** Customer & Vehicle Management (full stack), Admin Dashboard, Reporting & Analytics (full stack).

**Samia →** Public Status Lookup Portal (full stack, incl. PDF export), Notification & Approval Management (full stack), status-badge standard, E2E/system testing coordination.

Workload: Emon ~30%, Rabib ~27%, Aftab ~22%, Samia ~21% — **Emon+Rabib 57%, Aftab+Samia 43%**, both within your specified targets.
