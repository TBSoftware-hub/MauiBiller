using MauiBiller.Configuration;
using MauiBiller.Core.Models;
using MauiBiller.Core.Services;
using MauiBiller.Navigation;

namespace MauiBiller.ViewModels;

public sealed class InviteTeamMembersPageViewModel(
    IWorkspaceSnapshotService snapshotService,
    INavigationService navigationService) : FeaturePageViewModel(navigationService, "Invite Team Members")
{
    protected override async Task LoadAsync()
    {
        var snapshot = await snapshotService.GetCurrentSnapshotAsync();
        var contributors = snapshot.Workspace.Members.Where(member => member.Role is WorkspaceMemberRole.Contributor).ToList();

        Summary = "Team invitation architecture now hangs off shared workspace models and service abstractions that can later swap from in-memory state to Firebase-backed collaboration.";
        ReplaceMetrics(
        [
            Metric("Contributors", contributors.Count.ToString(), string.Join(", ", contributors.Select(member => member.FullName))),
            Metric("Permission model", "Owner + contributor", "Contributors are limited to time-entry workflows")
        ]);
        ReplaceActions([]);
    }
}

public sealed class ClientsPageViewModel(
    IWorkspaceSnapshotService snapshotService,
    INavigationService navigationService) : FeaturePageViewModel(navigationService, "Clients")
{
    protected override async Task LoadAsync()
    {
        var snapshot = await snapshotService.GetCurrentSnapshotAsync();
        var activeClients = snapshot.Clients.Where(client => !client.IsArchived).ToList();

        Summary = "Client management now reads from repository abstractions and a shared workspace snapshot, which gives the rest of the app a stable model to build on.";
        ReplaceMetrics(
        [
            Metric("Active clients", activeClients.Count.ToString(), string.Join(", ", activeClients.Select(client => client.Name))),
            Metric("Archived clients", snapshot.Clients.Count(client => client.IsArchived).ToString(), "Available for later archive management flows"),
            Metric("Client contacts", activeClients.Count.ToString(), "Contact metadata lives in platform-independent core models")
        ]);
        ReplaceActions(
        [
            CreateNavigationAction("Open Client Details", "Inspect the seeded client-detail route.", AppRoutes.ClientDetails),
            CreateNavigationAction("Invite Team Members", "Jump to the workspace invitation flow.", AppRoutes.InviteTeamMembers)
        ]);
    }
}

public sealed class ClientDetailsPageViewModel(
    IWorkspaceSnapshotService snapshotService,
    INavigationService navigationService) : FeaturePageViewModel(navigationService, "Client Details")
{
    protected override async Task LoadAsync()
    {
        var snapshot = await snapshotService.GetCurrentSnapshotAsync();
        var client = snapshot.Clients.First(client => !client.IsArchived);
        var projectCount = snapshot.Projects.Count(project => project.ClientId == client.Id && !project.IsArchived);

        Summary = "Client details are wired to the shared domain layer so later pages can load the same client model without duplicating state or assumptions.";
        ReplaceMetrics(
        [
            Metric("Selected client", client.Name, client.ContactName),
            Metric("Projects", projectCount.ToString(), "Derived from the project repository abstraction"),
            Metric("Contact email", client.ContactEmail, "Ready for billing metadata and invoice recipients")
        ]);
        ReplaceActions([]);
    }
}

public sealed class ProjectsPageViewModel(
    IWorkspaceSnapshotService snapshotService,
    INavigationService navigationService) : FeaturePageViewModel(navigationService, "Projects")
{
    protected override async Task LoadAsync()
    {
        var snapshot = await snapshotService.GetCurrentSnapshotAsync();
        var activeProjects = snapshot.Projects.Where(project => !project.IsArchived).ToList();

        Summary = "Projects now sit on reusable core models and repository interfaces, which keeps the page independent from the eventual local-store and sync implementations.";
        ReplaceMetrics(
        [
            Metric("Active projects", activeProjects.Count.ToString(), string.Join(", ", activeProjects.Select(project => project.Name))),
            Metric("Average rate", FormatCurrency(activeProjects.Average(project => project.DefaultHourlyRate)), "Comes from platform-independent project models"),
            Metric("Work item coverage", snapshot.WorkItems.Count.ToString(), "Ready for time-entry and billing composition")
        ]);
        ReplaceActions(
        [
            CreateNavigationAction("Open Project Work Items", "Inspect the route used for project-specific work item management.", AppRoutes.ProjectWorkItems)
        ]);
    }
}

public sealed class ProjectWorkItemsPageViewModel(
    IWorkspaceSnapshotService snapshotService,
    INavigationService navigationService) : FeaturePageViewModel(navigationService, "Project Work Items")
{
    protected override async Task LoadAsync()
    {
        var snapshot = await snapshotService.GetCurrentSnapshotAsync();

        Summary = "Work items and rates are already represented in the core layer, which gives time tracking and billing a shared source of truth.";
        ReplaceMetrics(
        [
            Metric("Work items", snapshot.WorkItems.Count.ToString(), string.Join(", ", snapshot.WorkItems.Select(workItem => workItem.Name))),
            Metric("Highest rate", FormatCurrency(snapshot.WorkItems.Max(workItem => workItem.HourlyRate)), "Available to billing calculations later")
        ]);
        ReplaceActions([]);
    }
}

public sealed class TimerPageViewModel(
    IWorkspaceSnapshotService snapshotService,
    INavigationService navigationService) : FeaturePageViewModel(navigationService, "Timer")
{
    protected override async Task LoadAsync()
    {
        var snapshot = await snapshotService.GetCurrentSnapshotAsync();
        var totalDuration = snapshot.TimeEntries.Aggregate(TimeSpan.Zero, (current, entry) => current.Add(entry.Duration));

        Summary = "Time tracking now hangs off shared work-item and time-entry models, which gives both live timer and manual entry flows a consistent architecture.";
        ReplaceMetrics(
        [
            Metric("Seeded entries", snapshot.TimeEntries.Count.ToString(), "Repository-backed sample data"),
            Metric("Tracked hours", FormatHours(totalDuration), "Aggregated from platform-independent time-entry models"),
            Metric("Work item targets", snapshot.WorkItems.Count.ToString(), "Time entries reference work items instead of page-local state")
        ]);
        ReplaceActions(
        [
            CreateNavigationAction("Open Add Manual Time", "Review the manual-entry route that shares the same models.", AppRoutes.AddManualTime)
        ]);
    }
}

public sealed class AddManualTimePageViewModel(
    IWorkspaceSnapshotService snapshotService,
    INavigationService navigationService) : FeaturePageViewModel(navigationService, "Add Manual Time")
{
    protected override async Task LoadAsync()
    {
        var snapshot = await snapshotService.GetCurrentSnapshotAsync();
        var latestEntry = snapshot.TimeEntries.MaxBy(entry => entry.EntryDate);

        Summary = "Manual time entry reuses the same domain models as timer-based entry, so later validation and persistence work can stay in shared services.";
        ReplaceMetrics(
        [
            Metric("Latest entry date", latestEntry?.EntryDate.ToString("yyyy-MM-dd") ?? "None", latestEntry?.Notes ?? "No seeded entries"),
            Metric("Available contributors", snapshot.Workspace.Members.Count(member => member.Role is WorkspaceMemberRole.Contributor).ToString(), "Mirrors the invitation and permissions model")
        ]);
        ReplaceActions([]);
    }
}

public sealed class BillingPageViewModel(
    IWorkspaceSnapshotService snapshotService,
    INavigationService navigationService) : FeaturePageViewModel(navigationService, "Billing")
{
    protected override async Task LoadAsync()
    {
        var snapshot = await snapshotService.GetCurrentSnapshotAsync();
        var totalExpenses = snapshot.Expenses.Sum(expense => expense.Amount);
        var recurringInvoices = snapshot.InvoiceDrafts.Count(invoice => invoice.IsRecurring);

        Summary = "Billing now composes time entries, expenses, and invoice drafts through a shared snapshot service rather than page-specific placeholder text.";
        ReplaceMetrics(
        [
            Metric("Invoice drafts", snapshot.InvoiceDrafts.Count.ToString(), $"{recurringInvoices} recurring"),
            Metric("Expense total", FormatCurrency(totalExpenses), "Derived from the expense repository abstraction"),
            Metric("Billable entries", snapshot.TimeEntries.Count.ToString(), "Ready for later invoice aggregation logic")
        ]);
        ReplaceActions(
        [
            CreateNavigationAction("Open Create Invoice", "Inspect the draft invoice route built on the same architecture.", AppRoutes.CreateInvoice)
        ]);
    }
}

public sealed class CreateInvoicePageViewModel(
    IWorkspaceSnapshotService snapshotService,
    INavigationService navigationService) : FeaturePageViewModel(navigationService, "Create Invoice")
{
    protected override async Task LoadAsync()
    {
        var snapshot = await snapshotService.GetCurrentSnapshotAsync();
        var draft = snapshot.InvoiceDrafts.First();

        Summary = "Invoice creation now pulls from shared invoice, expense, and client models so later PDF generation can sit behind a clean service boundary.";
        ReplaceMetrics(
        [
            Metric("Draft number", draft.InvoiceNumber, FormatCurrency(draft.Amount)),
            Metric("Recurring support", snapshot.InvoiceDrafts.Count(invoice => invoice.IsRecurring).ToString(), "Seeded to match the MVP scope")
        ]);
        ReplaceActions([]);
    }
}

public sealed class SettingsPageViewModel(
    IWorkspaceSnapshotService snapshotService,
    INavigationService navigationService,
    AppConfiguration appConfiguration) : FeaturePageViewModel(navigationService, "Settings")
{
    protected override async Task LoadAsync()
    {
        var snapshot = await snapshotService.GetCurrentSnapshotAsync();

        Summary = "Settings now reflects real application configuration and shared workspace state, which keeps environment and diagnostics concerns out of the UI layer.";
        ReplaceMetrics(
        [
            Metric("Environment", appConfiguration.EnvironmentName, appConfiguration.Firebase.DatabaseUrl),
            Metric("Workspace", snapshot.Workspace.Name, snapshot.Workspace.OwnerName),
            Metric("Debug logging", appConfiguration.Diagnostics.EnableDebugLogging ? "Enabled" : "Disabled", "Controlled through embedded app settings")
        ]);
        ReplaceActions([]);
    }
}
