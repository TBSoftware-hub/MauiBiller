using MauiBiller.Core.Models;
using MauiBiller.Core.Repositories;

namespace MauiBiller.Infrastructure.Data;

public sealed class LocalWorkspaceRepository(LocalWorkspaceStore store) : IWorkspaceRepository
{
    public Task<Workspace> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return store.GetWorkspaceAsync(cancellationToken);
    }
}

public sealed class LocalClientRepository(LocalWorkspaceStore store) : IClientRepository
{
    public Task<IReadOnlyList<Client>> ListAsync(CancellationToken cancellationToken = default)
    {
        return store.ListClientsAsync(cancellationToken);
    }
}

public sealed class LocalProjectRepository(LocalWorkspaceStore store) : IProjectRepository
{
    public Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default)
    {
        return store.ListProjectsAsync(cancellationToken);
    }
}

public sealed class LocalWorkItemRepository(LocalWorkspaceStore store) : IWorkItemRepository
{
    public Task<IReadOnlyList<WorkItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        return store.ListWorkItemsAsync(cancellationToken);
    }
}

public sealed class LocalTimeEntryRepository(LocalWorkspaceStore store) : ITimeEntryRepository
{
    public Task<IReadOnlyList<TimeEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        return store.ListTimeEntriesAsync(cancellationToken);
    }
}

public sealed class LocalExpenseRepository(LocalWorkspaceStore store) : IExpenseRepository
{
    public Task<IReadOnlyList<Expense>> ListAsync(CancellationToken cancellationToken = default)
    {
        return store.ListExpensesAsync(cancellationToken);
    }
}

public sealed class LocalInvoiceDraftRepository(LocalWorkspaceStore store) : IInvoiceDraftRepository
{
    public Task<IReadOnlyList<InvoiceDraft>> ListAsync(CancellationToken cancellationToken = default)
    {
        return store.ListInvoiceDraftsAsync(cancellationToken);
    }
}
