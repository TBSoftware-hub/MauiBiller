using System.Collections.Concurrent;
using System.Globalization;
using MauiBiller.Core.Models;
using MauiBiller.Core.Services;
using SQLite;

namespace MauiBiller.Infrastructure.Data;

public sealed class LocalWorkspaceStore(
    InMemoryWorkspaceStore seedData,
    IAuthSessionService authSessionService)
{
    private const SQLiteOpenFlags ConnectionFlags =
        SQLiteOpenFlags.ReadWrite |
        SQLiteOpenFlags.Create |
        SQLiteOpenFlags.SharedCache;

    private readonly SemaphoreSlim initializationLock = new(1, 1);
    private readonly ConcurrentDictionary<string, SQLiteAsyncConnection> connections = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> initializedDatabasePaths = new(StringComparer.OrdinalIgnoreCase);

    public async Task<WorkspaceBootstrapResult> EnsureOwnerWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var user = GetCurrentUser();
        var (databasePath, connection) = GetConnectionForCurrentUser(user);
        await EnsureInitializedAsync(connection, databasePath, cancellationToken);

        var existingProfile = await connection.Table<OwnerProfileRecord>()
            .FirstOrDefaultAsync(profile => profile.UserId == user.UserId);

        if (existingProfile is not null)
        {
            existingProfile.Email = user.Email;
            existingProfile.DisplayName = GetOwnerDisplayName(user);
            existingProfile.LastSignedInUtcTicks = DateTimeOffset.UtcNow.UtcTicks;
            await connection.InsertOrReplaceAsync(existingProfile);

            var workspace = await LoadWorkspaceAsync(connection, existingProfile.WorkspaceId);
            return new WorkspaceBootstrapResult(workspace, existingProfile.ToModel(), false);
        }

        var snapshot = seedData.CreateOwnerBootstrapSnapshot(user);
        var ownerProfile = OwnerProfileRecord.Create(user, snapshot.Workspace.Id);

        await connection.InsertAsync(WorkspaceRecord.FromModel(snapshot.Workspace));
        await connection.InsertAllAsync(snapshot.Workspace.Members.Select(member => WorkspaceMemberRecord.FromModel(snapshot.Workspace.Id, member)).ToList());
        await connection.InsertAllAsync(snapshot.Clients.Select(ClientRecord.FromModel).ToList());
        await connection.InsertAllAsync(snapshot.Projects.Select(ProjectRecord.FromModel).ToList());
        await connection.InsertAllAsync(snapshot.WorkItems.Select(WorkItemRecord.FromModel).ToList());
        await connection.InsertAllAsync(snapshot.TimeEntries.Select(TimeEntryRecord.FromModel).ToList());
        await connection.InsertAllAsync(snapshot.Expenses.Select(ExpenseRecord.FromModel).ToList());
        await connection.InsertAllAsync(snapshot.InvoiceDrafts.Select(InvoiceDraftRecord.FromModel).ToList());
        await connection.InsertAsync(ownerProfile);

        return new WorkspaceBootstrapResult(snapshot.Workspace, ownerProfile.ToModel(), true);
    }

    public async Task<Workspace> GetWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var bootstrapResult = await EnsureOwnerWorkspaceAsync(cancellationToken);
        return bootstrapResult.Workspace;
    }

    public async Task<IReadOnlyList<Client>> ListClientsAsync(CancellationToken cancellationToken = default)
    {
        var (_, connection) = await EnsureCurrentConnectionAsync(cancellationToken);
        var records = await connection.Table<ClientRecord>().ToListAsync();
        return records.Select(record => record.ToModel()).ToList();
    }

    public async Task<IReadOnlyList<Project>> ListProjectsAsync(CancellationToken cancellationToken = default)
    {
        var (_, connection) = await EnsureCurrentConnectionAsync(cancellationToken);
        var records = await connection.Table<ProjectRecord>().ToListAsync();
        return records.Select(record => record.ToModel()).ToList();
    }

    public async Task<IReadOnlyList<WorkItem>> ListWorkItemsAsync(CancellationToken cancellationToken = default)
    {
        var (_, connection) = await EnsureCurrentConnectionAsync(cancellationToken);
        var records = await connection.Table<WorkItemRecord>().ToListAsync();
        return records.Select(record => record.ToModel()).ToList();
    }

    public async Task<IReadOnlyList<TimeEntry>> ListTimeEntriesAsync(CancellationToken cancellationToken = default)
    {
        var (_, connection) = await EnsureCurrentConnectionAsync(cancellationToken);
        var records = await connection.Table<TimeEntryRecord>().ToListAsync();
        return records.Select(record => record.ToModel()).ToList();
    }

    public async Task<IReadOnlyList<Expense>> ListExpensesAsync(CancellationToken cancellationToken = default)
    {
        var (_, connection) = await EnsureCurrentConnectionAsync(cancellationToken);
        var records = await connection.Table<ExpenseRecord>().ToListAsync();
        return records.Select(record => record.ToModel()).ToList();
    }

    public async Task<IReadOnlyList<InvoiceDraft>> ListInvoiceDraftsAsync(CancellationToken cancellationToken = default)
    {
        var (_, connection) = await EnsureCurrentConnectionAsync(cancellationToken);
        var records = await connection.Table<InvoiceDraftRecord>().ToListAsync();
        return records.Select(record => record.ToModel()).ToList();
    }

    private async Task<(AuthenticatedUser User, SQLiteAsyncConnection Connection)> EnsureCurrentConnectionAsync(CancellationToken cancellationToken)
    {
        var bootstrapResult = await EnsureOwnerWorkspaceAsync(cancellationToken);
        var user = new AuthenticatedUser(
            bootstrapResult.OwnerProfile.UserId,
            bootstrapResult.OwnerProfile.Email,
            bootstrapResult.OwnerProfile.DisplayName);
        return (user, GetConnectionForCurrentUser(user).Connection);
    }

    private (string DatabasePath, SQLiteAsyncConnection Connection) GetConnectionForCurrentUser(AuthenticatedUser user)
    {
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, $"mauibiller-{SanitizeFileNameSegment(user.UserId)}.db3");
        var connection = connections.GetOrAdd(databasePath, path => new SQLiteAsyncConnection(path, ConnectionFlags));
        return (databasePath, connection);
    }

    private async Task EnsureInitializedAsync(
        SQLiteAsyncConnection connection,
        string databasePath,
        CancellationToken cancellationToken)
    {
        if (initializedDatabasePaths.Contains(databasePath))
        {
            return;
        }

        await initializationLock.WaitAsync(cancellationToken);

        try
        {
            if (initializedDatabasePaths.Contains(databasePath))
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
            await connection.CreateTableAsync<OwnerProfileRecord>();

            initializedDatabasePaths.Add(databasePath);
        }
        finally
        {
            initializationLock.Release();
        }
    }

    private static string GetOwnerDisplayName(AuthenticatedUser user)
    {
        return string.IsNullOrWhiteSpace(user.DisplayName)
            ? user.Email.Split('@')[0]
            : user.DisplayName.Trim();
    }

    private static string SanitizeFileNameSegment(string input)
    {
        var invalidCharacters = Path.GetInvalidFileNameChars();
        return string.Concat(input.Select(character => invalidCharacters.Contains(character) ? '_' : character));
    }

    private AuthenticatedUser GetCurrentUser()
    {
        return authSessionService.CurrentUser
            ?? throw new InvalidOperationException("Workspace data requires an authenticated user.");
    }

    private static async Task<Workspace> LoadWorkspaceAsync(SQLiteAsyncConnection connection, string workspaceId)
    {
        var workspaceRecord = await connection.Table<WorkspaceRecord>()
            .FirstAsync(record => record.Id == workspaceId);
        var memberRecords = await connection.Table<WorkspaceMemberRecord>()
            .Where(member => member.WorkspaceId == workspaceRecord.Id)
            .ToListAsync();

        return workspaceRecord.ToModel(memberRecords.Select(member => member.ToModel()).ToList());
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

    [Table("owner_profile")]
    private sealed class OwnerProfileRecord
    {
        [PrimaryKey]
        public string UserId { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string WorkspaceId { get; set; } = string.Empty;

        public long FirstSignedInUtcTicks { get; set; }

        public long LastSignedInUtcTicks { get; set; }

        public WorkspaceOwnerProfile ToModel() =>
            new(
                UserId,
                Email,
                DisplayName,
                WorkspaceId,
                new DateTimeOffset(FirstSignedInUtcTicks, TimeSpan.Zero),
                new DateTimeOffset(LastSignedInUtcTicks, TimeSpan.Zero));

        public static OwnerProfileRecord Create(AuthenticatedUser user, string workspaceId)
        {
            var now = DateTimeOffset.UtcNow;

            return new OwnerProfileRecord
            {
                UserId = user.UserId,
                Email = user.Email,
                DisplayName = GetOwnerDisplayName(user),
                WorkspaceId = workspaceId,
                FirstSignedInUtcTicks = now.UtcTicks,
                LastSignedInUtcTicks = now.UtcTicks
            };
        }
    }
}
