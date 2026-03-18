using System.Globalization;
using MauiBiller.Core.Models;
using SQLite;

namespace MauiBiller.Infrastructure.Data;

public sealed class LocalWorkspaceStore(InMemoryWorkspaceStore seedData)
{
    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private readonly SQLiteAsyncConnection connection = new(
        Path.Combine(FileSystem.AppDataDirectory, "mauibiller.db3"),
        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

    private bool isInitialized;

    public async Task<Workspace> GetWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var workspaceRecord = await connection.Table<WorkspaceRecord>().FirstAsync();
        var memberRecords = await connection.Table<WorkspaceMemberRecord>()
            .Where(member => member.WorkspaceId == workspaceRecord.Id)
            .ToListAsync();

        return workspaceRecord.ToModel(memberRecords.Select(member => member.ToModel()).ToList());
    }

    public async Task<IReadOnlyList<Client>> ListClientsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var records = await connection.Table<ClientRecord>().ToListAsync();
        return records.Select(record => record.ToModel()).ToList();
    }

    public async Task<IReadOnlyList<Project>> ListProjectsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var records = await connection.Table<ProjectRecord>().ToListAsync();
        return records.Select(record => record.ToModel()).ToList();
    }

    public async Task<IReadOnlyList<WorkItem>> ListWorkItemsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var records = await connection.Table<WorkItemRecord>().ToListAsync();
        return records.Select(record => record.ToModel()).ToList();
    }

    public async Task<IReadOnlyList<TimeEntry>> ListTimeEntriesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var records = await connection.Table<TimeEntryRecord>().ToListAsync();
        return records.Select(record => record.ToModel()).ToList();
    }

    public async Task<IReadOnlyList<Expense>> ListExpensesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var records = await connection.Table<ExpenseRecord>().ToListAsync();
        return records.Select(record => record.ToModel()).ToList();
    }

    public async Task<IReadOnlyList<InvoiceDraft>> ListInvoiceDraftsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        var records = await connection.Table<InvoiceDraftRecord>().ToListAsync();
        return records.Select(record => record.ToModel()).ToList();
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (isInitialized)
        {
            return;
        }

        await initializationLock.WaitAsync(cancellationToken);

        try
        {
            if (isInitialized)
            {
                return;
            }

            await connection.CreateTableAsync<WorkspaceRecord>();
            await connection.CreateTableAsync<WorkspaceMemberRecord>();
            await connection.CreateTableAsync<ClientRecord>();
            await connection.CreateTableAsync<ProjectRecord>();
            await connection.CreateTableAsync<WorkItemRecord>();
            await connection.CreateTableAsync<TimeEntryRecord>();
            await connection.CreateTableAsync<ExpenseRecord>();
            await connection.CreateTableAsync<InvoiceDraftRecord>();

            if (await connection.Table<WorkspaceRecord>().CountAsync() == 0)
            {
                await SeedAsync();
            }

            isInitialized = true;
        }
        finally
        {
            initializationLock.Release();
        }
    }

    private async Task SeedAsync()
    {
        await connection.InsertAsync(WorkspaceRecord.FromModel(seedData.Workspace));
        await connection.InsertAllAsync(seedData.Workspace.Members.Select(member => WorkspaceMemberRecord.FromModel(seedData.Workspace.Id, member)).ToList());
        await connection.InsertAllAsync(seedData.Clients.Select(ClientRecord.FromModel).ToList());
        await connection.InsertAllAsync(seedData.Projects.Select(ProjectRecord.FromModel).ToList());
        await connection.InsertAllAsync(seedData.WorkItems.Select(WorkItemRecord.FromModel).ToList());
        await connection.InsertAllAsync(seedData.TimeEntries.Select(TimeEntryRecord.FromModel).ToList());
        await connection.InsertAllAsync(seedData.Expenses.Select(ExpenseRecord.FromModel).ToList());
        await connection.InsertAllAsync(seedData.InvoiceDrafts.Select(InvoiceDraftRecord.FromModel).ToList());
    }

    [Table("workspace")]
    private sealed class WorkspaceRecord
    {
        [PrimaryKey]
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string OwnerName { get; set; } = string.Empty;

        public string OwnerEmail { get; set; } = string.Empty;

        public Workspace ToModel(IReadOnlyList<WorkspaceMember> members) =>
            new(Id, Name, OwnerName, OwnerEmail, members);

        public static WorkspaceRecord FromModel(Workspace workspace) => new()
        {
            Id = workspace.Id,
            Name = workspace.Name,
            OwnerName = workspace.OwnerName,
            OwnerEmail = workspace.OwnerEmail
        };
    }

    [Table("workspace_member")]
    private sealed class WorkspaceMemberRecord
    {
        [PrimaryKey]
        public string Id { get; set; } = string.Empty;

        [Indexed]
        public string WorkspaceId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public WorkspaceMember ToModel() =>
            new(Id, FullName, Email, Enum.Parse<WorkspaceMemberRole>(Role, ignoreCase: true));

        public static WorkspaceMemberRecord FromModel(string workspaceId, WorkspaceMember member) => new()
        {
            Id = member.Id,
            WorkspaceId = workspaceId,
            FullName = member.FullName,
            Email = member.Email,
            Role = member.Role.ToString()
        };
    }

    [Table("client")]
    private sealed class ClientRecord
    {
        [PrimaryKey]
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string ContactName { get; set; } = string.Empty;

        public string ContactEmail { get; set; } = string.Empty;

        public bool IsArchived { get; set; }

        public Client ToModel() => new(Id, Name, ContactName, ContactEmail, IsArchived);

        public static ClientRecord FromModel(Client client) => new()
        {
            Id = client.Id,
            Name = client.Name,
            ContactName = client.ContactName,
            ContactEmail = client.ContactEmail,
            IsArchived = client.IsArchived
        };
    }

    [Table("project")]
    private sealed class ProjectRecord
    {
        [PrimaryKey]
        public string Id { get; set; } = string.Empty;

        [Indexed]
        public string ClientId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public decimal DefaultHourlyRate { get; set; }

        public bool IsArchived { get; set; }

        public Project ToModel() => new(Id, ClientId, Name, DefaultHourlyRate, IsArchived);

        public static ProjectRecord FromModel(Project project) => new()
        {
            Id = project.Id,
            ClientId = project.ClientId,
            Name = project.Name,
            DefaultHourlyRate = project.DefaultHourlyRate,
            IsArchived = project.IsArchived
        };
    }

    [Table("work_item")]
    private sealed class WorkItemRecord
    {
        [PrimaryKey]
        public string Id { get; set; } = string.Empty;

        [Indexed]
        public string ProjectId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public decimal HourlyRate { get; set; }

        public WorkItem ToModel() => new(Id, ProjectId, Name, HourlyRate);

        public static WorkItemRecord FromModel(WorkItem workItem) => new()
        {
            Id = workItem.Id,
            ProjectId = workItem.ProjectId,
            Name = workItem.Name,
            HourlyRate = workItem.HourlyRate
        };
    }

    [Table("time_entry")]
    private sealed class TimeEntryRecord
    {
        [PrimaryKey]
        public string Id { get; set; } = string.Empty;

        [Indexed]
        public string WorkItemId { get; set; } = string.Empty;

        [Indexed]
        public string MemberId { get; set; } = string.Empty;

        public string EntryDate { get; set; } = string.Empty;

        public long DurationTicks { get; set; }

        public string Notes { get; set; } = string.Empty;

        public TimeEntry ToModel() =>
            new(
                Id,
                WorkItemId,
                MemberId,
                DateOnly.ParseExact(EntryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                new TimeSpan(DurationTicks),
                Notes);

        public static TimeEntryRecord FromModel(TimeEntry timeEntry) => new()
        {
            Id = timeEntry.Id,
            WorkItemId = timeEntry.WorkItemId,
            MemberId = timeEntry.MemberId,
            EntryDate = timeEntry.EntryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DurationTicks = timeEntry.Duration.Ticks,
            Notes = timeEntry.Notes
        };
    }

    [Table("expense")]
    private sealed class ExpenseRecord
    {
        [PrimaryKey]
        public string Id { get; set; } = string.Empty;

        [Indexed]
        public string ClientId { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public Expense ToModel() => new(Id, ClientId, Description, Amount);

        public static ExpenseRecord FromModel(Expense expense) => new()
        {
            Id = expense.Id,
            ClientId = expense.ClientId,
            Description = expense.Description,
            Amount = expense.Amount
        };
    }

    [Table("invoice_draft")]
    private sealed class InvoiceDraftRecord
    {
        [PrimaryKey]
        public string Id { get; set; } = string.Empty;

        [Indexed]
        public string ClientId { get; set; } = string.Empty;

        public string InvoiceNumber { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public bool IsRecurring { get; set; }

        public InvoiceDraft ToModel() => new(Id, ClientId, InvoiceNumber, Amount, IsRecurring);

        public static InvoiceDraftRecord FromModel(InvoiceDraft invoiceDraft) => new()
        {
            Id = invoiceDraft.Id,
            ClientId = invoiceDraft.ClientId,
            InvoiceNumber = invoiceDraft.InvoiceNumber,
            Amount = invoiceDraft.Amount,
            IsRecurring = invoiceDraft.IsRecurring
        };
    }
}
