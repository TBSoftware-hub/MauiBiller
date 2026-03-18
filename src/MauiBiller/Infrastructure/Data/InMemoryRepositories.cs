using MauiBiller.Core.Models;
using MauiBiller.Core.Repositories;

namespace MauiBiller.Infrastructure.Data;

public sealed class InMemoryWorkspaceRepository(InMemoryWorkspaceStore store) : IWorkspaceRepository
{
    public Task<Workspace> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.Workspace);
    }
}

public sealed class InMemoryClientRepository(InMemoryWorkspaceStore store) : IClientRepository
{
    public Task<IReadOnlyList<Client>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.Clients);
    }
}

public sealed class InMemoryProjectRepository(InMemoryWorkspaceStore store) : IProjectRepository
{
    public Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.Projects);
    }
}

public sealed class InMemoryWorkItemRepository(InMemoryWorkspaceStore store) : IWorkItemRepository
{
    public Task<IReadOnlyList<WorkItem>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.WorkItems);
    }
}

public sealed class InMemoryTimeEntryRepository(InMemoryWorkspaceStore store) : ITimeEntryRepository
{
    public Task<IReadOnlyList<TimeEntry>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.TimeEntries);
    }
}

public sealed class InMemoryExpenseRepository(InMemoryWorkspaceStore store) : IExpenseRepository
{
    public Task<IReadOnlyList<Expense>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.Expenses);
    }
}

public sealed class InMemoryInvoiceDraftRepository(InMemoryWorkspaceStore store) : IInvoiceDraftRepository
{
    public Task<IReadOnlyList<InvoiceDraft>> ListAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(store.InvoiceDrafts);
    }
}
