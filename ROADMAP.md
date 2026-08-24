# Garij — Development Roadmap

**Course:** CSE 3200 — Software Development V
**Team:** Group 4, Lab Section C1
**Repository:** https://github.com/AftabAhmedFahim/garij

This document divides the remaining work into three stages. Each stage contains
tasks for all four members, sized so that nobody is blocked waiting on someone
else within the same stage.

---

## Module Ownership

Each member owns a vertical slice for the whole project. Owning a slice means you
write the service, the repository methods, the controller, and the views for that
area — and you are the person who reviews any PR that touches it.

| Member | Owned Modules | Primary Files |
|---|---|---|
| **Samia Tabassum** | Customer & Vehicle, Public Status Lookup | `CustomerVehicleService`, `Customer/Vehicle/StatusLookup` controllers + views |
| **Rakibul Islam Emon** | Service Jobs, Mechanic Assignment | `ServiceJobService`, `ServiceJob/Mechanic` controllers + views |
| **Rubaiat Ar Rabib** | Parts & Inventory, Notifications | `PartsInventoryService`, `NotificationService`, `Parts/Notification` controllers + views |
| **Aftab Ahmed Fahim** | Billing & Invoicing, Reporting, Intelligence | `BillingService`, `ReportingService`, `IntelligenceService`, `Billing/Report` controllers + views |

### Shared files — coordinate before editing

These are touched by everyone and are the most likely source of merge conflicts.
Announce in the group chat before changing them:

- `Program.cs`
- `GarijDbContext.cs`
- `_Layout.cshtml`
- `appsettings.json`
- Anything in `src/Garij.Domain/Entities/`

### Working Agreement

- `main` is protected. No direct pushes.
- Branch naming: `feature/<module>-<short-description>` (e.g. `feature/billing-invoice-generation`).
- Every PR needs one approval from a teammate before merge.
- Pull from `main` before starting work each day.
- Never commit migrations without telling the team — migration conflicts are painful to untangle.

---

## Stage 0 — Blockers (do these first, together)

These must be resolved before Stage 1 begins. Sit together for one session and
settle them; they affect everyone's code.

| # | Decision | Notes |
|---|---|---|
| 1 | **Identity model** | Currently there are two user tables: Identity's `AspNetUsers` and the domain `User` (`StaffUsers`), joined by a plain `IdentityUserId` string with no FK. Decide: keep them separate with a proper FK, or make `User` inherit `IdentityUser`. This blocks Admin and Mechanic work. |
| 2 | **Customer → ServiceJob link** | The report ERD shows a direct relationship; the code reaches Customer through Vehicle. Either add `CustomerId` to `ServiceJob` to match the report, or record the deviation so it can be explained during evaluation. |
| 3 | **Default route** | `Program.cs` currently opens on `StatusLookup`. Change to `Dashboard` if staff should land on login first. |
| 4 | **Solution file format** | `Garij.slnx` needs Visual Studio 17.14+. Confirm everyone can open it, or regenerate a classic `.sln`. |
| 5 | **First migration** | One person creates the initial EF Core migration and runs `database update`. Everyone else pulls it — nobody generates a competing one. |

---

## Stage 1 — Foundation & Core CRUD

**Goal:** Database is live, login works, and every entity can be created and listed
through the UI. At the end of this stage the app runs end to end, even if it is plain.

### Everyone

- Pull the initial migration and confirm `dotnet run` works locally.
- Implement repository methods for your own entities (the generic `Repository<T>` is already there).
- Build Index / Create / Edit / Details / Delete views for your entities using scaffolded Bootstrap forms. Styling comes later.

### Samia — Customer & Vehicle

- Implement `CustomerVehicleService`: register customer, add vehicle to customer, search by plate number, retrieve service history.
- Build `CustomerController` and `VehicleController` CRUD actions.
- Add validation attributes: required fields, email format, phone format, unique licence plate.
- **Covers:** FR-1, FR-2, FR-4

### Rakibul — Service Jobs & Mechanics

- Implement `ServiceJobService`: create job, assign mechanic, list jobs by status.
- Generate a unique `BookingReference` on job creation (short, human-readable — e.g. `GRJ-2026-0001`).
- Build `ServiceJobController` create and list actions, and `MechanicController` assignment actions.
- Seed the `ServiceCatalog` table with 8–10 realistic services and base costs.
- **Covers:** FR-3 (partial)

### Rubaiat — Parts & Inventory

- Implement `PartsInventoryService`: add part, list parts, update stock quantity.
- Build `PartsController` CRUD actions.
- Seed the `Part` table with 15–20 realistic automotive parts, including unit price, stock quantity, and reorder level.
- Add stock-quantity validation (never negative).
- **Covers:** FR-9 (foundation)

### Aftab — Auth, Layout & Billing Foundation

- Finish the Stage 0 identity decision in code; implement `AccountController` login, logout, and register.
- Apply `[Authorize(Roles = ...)]` across every controller; verify a mechanic cannot reach billing.
- Seed one Admin account so the team has a working login.
- Build `_Layout.cshtml` with role-aware navigation, and the three role dashboards in `DashboardController`.
- Implement `BillingService.GenerateInvoice` skeleton (structure only, calculation lands in Stage 2).
- **Covers:** FR-19, role-based access for all FRs

### Stage 1 Exit Criteria

- [ ] Migration applied; database has seeded catalog, parts, and an Admin user
- [ ] Login works and routes to the correct dashboard per role
- [ ] A customer, a vehicle, a service job, and a part can each be created through the UI
- [ ] Mechanics cannot access billing or admin pages
- [ ] `dotnet build` and `dotnet test` pass; CI is green on `main`

---

## Stage 2 — Business Logic & Workflows

**Goal:** The system enforces real rules. Jobs move through validated states, stock
decrements automatically, invoices calculate correctly, and customers can check status.

### Samia — Public Status Lookup & Service History

- Implement `StatusLookupController`: accept plate number **or** booking reference, return current job status.
- Build a clean status timeline view showing the job's progression through statuses.
- Handle the not-found case gracefully (no stack traces to the public).
- Add full service-history view per vehicle, ordered by date.
- Make this page mobile-first — customers open it on phones.
- **Covers:** FR-11, FR-4

### Rakibul — Job Status State Machine

- Implement `ValidateStatusTransition` in the service layer. Legal transitions only:
  `Requested → InspectionPending → CustomerApprovalNeeded → InProgress → Completed`, with `Cancelled` reachable from any non-completed state.
- **Hard rule:** a job cannot move to `Completed` unless all parts used have been logged. Coordinate with Rubaiat on the check.
- Build the mechanic job board: assigned jobs only, large touch targets, minimal typing.
- Implement diagnostic-notes recording against a job.
- Enforce lead-mechanic uniqueness — exactly one `RoleInJob.Lead` per job.
- **Covers:** FR-7, FR-8, FR-10, FR-3 (complete)

### Rubaiat — Parts Logging & Notifications

- Implement `LogPartsUsed`: record `JobPartUsed` with `UnitPriceAtTime` captured at logging time, and decrement `Part.StockQuantity` atomically.
- Raise a reorder alert when stock falls to or below `ReorderLevel`; surface it on the admin dashboard.
- Implement `NotificationService`: create a Pending notification when a job hits `Completed`.
- Build the notification approval queue — staff approve or reject before dispatch.
- **Covers:** FR-9, FR-13, FR-16, FR-22

### Aftab — Billing & Transactions

- Implement `GenerateInvoice`: total labour cost from `JobServiceDetail` plus parts cost from `JobPartUsed`, producing `SubTotal`, `TaxAmount`, `TotalAmount`.
- Wrap invoice generation and the `Completed` status transition in an **EF Core transaction with rollback**, so a mid-operation failure cannot deduct stock without producing an invoice. This is called out as a top risk in Report 02.
- Implement `RecordPayment` supporting full and partial payments across multiple methods; update `PaymentStatus` accordingly.
- Build the itemized invoice view.
- **Covers:** FR-5, FR-6, FR-12 (partial)

### Stage 2 Exit Criteria

- [ ] Invalid status transitions are rejected with a clear message
- [ ] A job cannot be completed before parts are logged
- [ ] Logging a part decrements stock; hitting the reorder level raises an alert
- [ ] Invoice totals are arithmetically correct on a manually verified test job
- [ ] Killing the app mid-transaction leaves no partial invoice (test this deliberately)
- [ ] A customer can look up a real job by plate number and by booking reference
- [ ] Full happy path works: register customer → create job → assign mechanic → log parts → complete → invoice → payment → notification

---

## Stage 3 — Intelligence, Reporting & Polish

**Goal:** The "intelligent" features promised in the proposal are working, reports
run, the UI is presentable, and the project is submission-ready.

### Samia — UI/UX Consistency Pass

- Apply the design system across every view: consistent buttons, tables, form layouts, spacing.
- Make job status colour-coded and readable at a glance — this is the most important information in the product.
- Add empty states, loading states, and friendly validation-error displays.
- Verify the public lookup page works properly on a real phone.
- Keep all labels short so Bangla translation would not break layouts later.

### Rakibul — Service-Due Flags & Job Board Polish

- Implement `IIntelligenceService.FlagVehiclesDueForService` using each vehicle's own recorded service history.
- Surface due-for-service vehicles on the front-desk dashboard.
- Add filtering and sorting to the job board (by status, by mechanic, by date).
- Write integration tests for the status state machine, including the illegal transitions.

### Rubaiat — Testing & Data Integrity

- Write unit tests for parts logging and stock decrement, including the concurrent-decrement edge case.
- Write unit tests for the notification approval flow.
- Add database-level constraints as a second line of defence: non-negative stock, required fields.
- Seed a realistic demo dataset (20+ customers, 30+ vehicles, 50+ jobs in mixed states) for the presentation.

### Aftab — Reporting & Smart Intake

- Implement `IReportingService`: monthly revenue, part consumption, mechanic workload.
- Build report views with printable output.
- Implement `IIntelligenceService.EstimateCompletionTime` from recorded durations of previous jobs of the same type.
- Implement the Gemini-backed smart intake assistant: pass `ServiceCatalog` entries as grounding context, return suggested service categories. Every suggestion must render as **advisory**, requiring explicit staff confirmation, and every request/response pair must be logged.
- Add a `GeminiApiKey` entry to `appsettings.json` — use user secrets locally and **never commit the real key**.
- **Covers:** FR-14, FR-15, FR-17, FR-18, FR-20, FR-21

### Everyone — Final Week

- Full regression pass on the happy path plus every error path.
- Update `README.md` with real setup steps and screenshots.
- Confirm all 22 functional requirements from the proposal are demonstrable.
- Rehearse the demo: assign who drives which part.
- Tag a release commit before the submission deadline.

### Stage 3 Exit Criteria

- [ ] All 22 FRs from the proposal are implemented and demonstrable
- [ ] Reports generate correctly against the seeded dataset
- [ ] Smart intake returns catalog-grounded suggestions and requires confirmation
- [ ] No API keys, passwords, or connection secrets in the repository
- [ ] All tests pass; CI is green
- [ ] Demo rehearsed end to end without crashes

---

## Requirement Coverage Map

Cross-check before submission — every FR from Section 4 of the proposal should appear here.

| Stage | Requirements Covered |
|---|---|
| Stage 1 | FR-1, FR-2, FR-4, FR-19 |
| Stage 2 | FR-3, FR-5, FR-6, FR-7, FR-8, FR-9, FR-10, FR-11, FR-13, FR-16, FR-22 |
| Stage 3 | FR-12, FR-14, FR-15, FR-17, FR-18, FR-20, FR-21 |

---

## Risk Watch

Carried over from Report 02 — check these at the end of every stage.

| Risk | Watch For | Mitigation |
|---|---|---|
| Scope creep | New features appearing mid-stage | Requirements are frozen at the 22 FRs. Route additions through the group. |
| Atomic billing failure | Partial invoices, stock deducted without a bill | Transaction wrapping in Stage 2 + a deliberate mid-transaction failure test |
| Architecture erosion | Controllers calling `DbContext` directly | Enforce at code review — controllers talk to services only |
| Integration crunch | Modules that only meet in the final week | The Stage 2 happy-path check forces integration early |
| Tooling mismatch | "Works on my machine" | Same SDK version for everyone; CI is the source of truth |
