namespace MauiBiller.Core.Models;

public enum WorkspaceMemberRole
{
    Owner,
    Contributor
}

public sealed record WorkspaceMember(
    string Id,
    string FullName,
    string Email,
    WorkspaceMemberRole Role);

public sealed record Workspace(
    string Id,
    string Name,
    string OwnerName,
    string OwnerEmail,
    IReadOnlyList<WorkspaceMember> Members);

public sealed record Client(
    string Id,
    string Name,
    string ContactName,
    string ContactEmail,
    bool IsArchived);

public sealed record Project(
    string Id,
    string ClientId,
    string Name,
    decimal DefaultHourlyRate,
    bool IsArchived);

public sealed record WorkItem(
    string Id,
    string ProjectId,
    string Name,
    decimal HourlyRate);

public sealed record TimeEntry(
    string Id,
    string WorkItemId,
    string MemberId,
    DateOnly EntryDate,
    TimeSpan Duration,
    string Notes);

public sealed record Expense(
    string Id,
    string ClientId,
    string Description,
    decimal Amount);

public sealed record InvoiceDraft(
    string Id,
    string ClientId,
    string InvoiceNumber,
    decimal Amount,
    bool IsRecurring);

public sealed record WorkspaceSnapshot(
    Workspace Workspace,
    IReadOnlyList<Client> Clients,
    IReadOnlyList<Project> Projects,
    IReadOnlyList<WorkItem> WorkItems,
    IReadOnlyList<TimeEntry> TimeEntries,
    IReadOnlyList<Expense> Expenses,
    IReadOnlyList<InvoiceDraft> InvoiceDrafts);

public sealed record WorkspaceOwnerProfile(
    string UserId,
    string Email,
    string DisplayName,
    string WorkspaceId,
    DateTimeOffset FirstSignedInAtUtc,
    DateTimeOffset LastSignedInAtUtc);

public sealed record WorkspaceBootstrapResult(
    Workspace Workspace,
    WorkspaceOwnerProfile OwnerProfile,
    bool IsFirstSignIn);
