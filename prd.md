# MauiBiller Product Requirements Document

## 1. Executive Summary

- **Problem Statement**: Technical service providers such as freelancers, agencies, and internal consulting teams often manage clients, projects, time tracking, and invoices across disconnected tools or spreadsheets. This creates delayed invoicing, missed billable time, inconsistent records across devices, and avoidable administrative overhead.
- **Proposed Solution**: Build `MauiBiller`, a local-first `.NET 10` MAUI application for Windows, macOS, iOS, and Android that lets users manage clients, projects, work items, rates, time entries, expenses, and PDF invoices in one product. The app will authenticate through Firebase Auth, persist core data locally for offline use, and synchronize changes to a Firebase-backed cloud database when connectivity is available.
- **Success Criteria**:
  - Reduce invoice preparation time by at least 50% compared with the team's current workflow.
  - Reduce missed billable time by at least 30% within the first billing cycle after adoption.
  - Achieve at least 99% successful sync operations without manual intervention.
  - Allow core MVP workflows to function consistently on Windows, macOS, iOS, and Android.

## 2. User Experience & Functionality

### User Personas

- **Independent Consultant**
  - A solo technical professional who needs a simple way to track billable work, manage clients, and issue invoices quickly from desktop or mobile.
- **Agency Owner or Team Lead**
  - A small software agency lead who manages clients, projects, rates, and billing while inviting team members to contribute billable time.
- **Internal Consulting Team Contributor**
  - A technical team member who primarily needs to log time accurately against the correct project and work item while offline or on the move.

### User Stories

#### Story 1: Authentication and Account Access

As a workspace owner, I want to register, sign in, and recover access to my account so that I can securely access my workspace across supported devices.

**Acceptance Criteria**

- Users can register a new account using the Firebase Auth-backed OAuth flow.
- Users can sign in on Windows, macOS, iOS, and Android.
- Users can trigger a password reset flow from the login experience.
- Authenticated sessions persist securely between app launches until sign-out or token expiry.
- Authentication failures return clear, actionable error messages without exposing sensitive details.

#### Story 2: Workspace and Team Invitations

As a workspace owner, I want to invite team members so that they can contribute time entries to shared client work.

**Acceptance Criteria**

- A workspace owner can send an invitation to a team member from the app.
- Invited members can accept an invitation and join the owner's workspace.
- The MVP permission model restricts invited members to time-entry capabilities only.
- Only the workspace owner can manage member invitations and broader workspace settings.
- The app clearly distinguishes owner actions from invited-member actions in the UI.

#### Story 3: Client Management

As a workspace owner, I want to create and manage clients so that all billable work can be organized under the correct customer.

**Acceptance Criteria**

- The owner can create, edit, archive, and view clients.
- Each client record can store the minimum billing metadata needed for invoicing, including name and contact details.
- The `Clients` screen lists all active clients and supports navigation to `Client Details`.
- Changes made offline are saved locally and queued for sync automatically.
- Client updates sync to the cloud when connectivity returns without requiring the user to re-enter data.

#### Story 4: Project, Work Item, and Rate Management

As a workspace owner, I want to define projects, work items, and agreed rates for each client so that time and invoices reflect the correct billable structure.

**Acceptance Criteria**

- The owner can create and edit projects under a client.
- The owner can create and edit project work items associated with a project.
- The owner can assign a billable rate to a project, work item, or other agreed billing unit defined by the implementation.
- Project and work item data are available offline after initial sync or local creation.
- The UI prevents time entries from being logged without an associated client/project/work item reference.

#### Story 5: Timer-Based and Manual Time Entry

As a user, I want to track time with a timer or enter time manually so that my billable work is captured accurately in the way that best fits my workflow.

**Acceptance Criteria**

- The app includes a dedicated `Timer` screen for starting, stopping, pausing, and saving a time entry.
- The app includes an `Add Manual Time` screen for entering duration, date, and associated work details without using the timer.
- Invited members can create and edit their own time entries, subject to the MVP permission model.
- Time entries created offline are stored locally and synchronized automatically later.
- The average time to capture a basic time entry should support rapid entry and not require unnecessary navigation.

#### Story 6: Billing, Expenses, and Recurring Invoices

As a workspace owner, I want to review billable activity, add basic expenses, and configure recurring invoices so that regular billing can be prepared with minimal manual effort.

**Acceptance Criteria**

- The `Billing` experience aggregates billable time and basic expenses by client and project context.
- The owner can create and edit basic expense entries associated with billable work or clients.
- The owner can create recurring invoice definitions for repeating billing scenarios.
- Recurring invoice setup supports at least the minimum schedule information required to generate future invoices.
- Expense tracking remains basic in MVP and does not require advanced accounting workflows.

#### Story 7: Invoice Creation and PDF Output

As a workspace owner, I want to create invoices in PDF format so that I can send professional billing documents to clients quickly.

**Acceptance Criteria**

- The owner can create an invoice from billable time, expenses, and recurring invoice data.
- The app generates a PDF invoice file suitable for sharing outside the app.
- Invoice totals exclude tax calculation in the MVP.
- The invoice creation flow supports offline drafting; if final PDF generation requires connectivity or platform services, the app must clearly communicate that requirement.
- Users can review invoice details before finalizing the PDF.

#### Story 8: Settings, Sync, and Cross-Platform Consistency

As a user, I want reliable settings and sync behavior so that I can trust the app across all of my devices.

**Acceptance Criteria**

- The `Settings` screen exposes account/session controls and sync-related status information appropriate for the user's role.
- The app supports full offline create/edit for core MVP data: clients, projects, work items, time entries, expenses, and draft billing data.
- The app attempts automatic synchronization when connectivity is restored.
- The app surfaces sync errors or conflicts in a user-visible way instead of silently discarding changes.
- Core MVP workflows behave consistently across Windows, macOS, iOS, and Android, even if minor platform-specific UI differences exist.

### Non-Goals

- Client portal access
- Tax calculation
- Online payment processing
- Advanced accounting or bookkeeping workflows
- Fine-grained custom role-based permissions beyond owner and time-entry-only members
- AI-assisted invoice generation, AI-assisted time classification, or other AI-powered features in MVP

## 3. AI System Requirements (If Applicable)

Not applicable for the MVP. No AI-powered features are included in the current scope.

## 4. Technical Specifications

### Architecture Overview

`MauiBiller` will be a `.NET 10` MAUI application targeting Windows, macOS, iOS, and Android with a local-first architecture.

High-level component flow:

1. **MAUI client application**
   - Presents the user interface for authentication, workspace management, time capture, billing, invoice creation, and settings.
2. **Local persistence layer**
   - Stores core workspace data locally so the app can support offline create/edit flows.
   - Maintains a local change queue or equivalent sync state for writes created while offline.
3. **Sync orchestration layer**
   - Detects connectivity restoration, synchronizes local changes to the cloud datastore, and reconciles remote updates down to the device.
   - Tracks sync state, failures, and retry behavior.
4. **Firebase services**
   - **Firebase Auth** handles authentication and session identity.
   - **Firebase Database** stores synchronized workspace data. The exact Firebase database product is `TBD` and should be finalized during technical design if needed.
5. **PDF generation component**
   - Produces invoice PDFs from finalized invoice data using a cross-platform-compatible PDF generation strategy.

### Integration Points

- **Firebase Auth**
  - OAuth-backed authentication flow
  - Session management and token refresh
  - Password reset support
- **Firebase Database**
  - Synchronized storage for clients, projects, work items, time entries, expenses, billing records, invitations, and invoice metadata
  - Offline-to-online reconciliation strategy
- **PDF generation**
  - Cross-platform invoice PDF rendering
  - Export and share workflow appropriate to each target platform

### Security & Privacy

- Authentication tokens must be stored securely using platform-appropriate secure storage mechanisms.
- Local billing and client data must be protected against casual exposure on the device.
- Sync operations must respect user authorization boundaries, especially between owner-only data management actions and invited-member time entry access.
- The app must not silently discard local changes during sync failures or conflicts.
- Sensitive client information included in invoices, client records, and billing metadata must be handled according to least-privilege principles.

## 5. Risks & Roadmap

### Phased Rollout

- **MVP**
  - Register, login, reset password
  - Owner workspace creation
  - Team invitations
  - Clients, client details, projects, work items, and rate management
  - Timer-based and manual time entry
  - Billing overview
  - Basic expense tracking
  - Recurring invoices
  - Create invoice and PDF export
  - Local-first storage with automatic Firebase sync
  - Settings and sync visibility

- **v1.1**
  - Improved conflict resolution UX for sync issues
  - Better invoice templates and customization
  - Stronger reporting around billable time and overdue billing activity
  - Enhanced recurring invoice management and scheduling controls

- **v2.0**
  - Richer team and permission models
  - Client-facing capabilities if later approved
  - Tax and accounting integrations if later approved
  - Payment integrations if later approved

### Technical Risks

- **Offline sync complexity**
  - Local-first behavior across four platforms creates risk around merge conflicts, stale data, retries, and duplicate writes.
- **Cross-platform parity**
  - MAUI behavior, native platform services, and file-sharing flows may vary between desktop and mobile targets.
- **PDF consistency**
  - PDF rendering and export behavior may differ across platforms if not designed around a consistent generation pipeline.
- **Firebase model choice**
  - The exact Firebase database product and data-shape strategy remain implementation decisions that could affect sync design and performance.
- **Permission enforcement**
  - Restricting invited members to time-entry-only behavior must be enforced consistently in both UI and synchronized data operations.
