using MauiBiller.Core.Models;

namespace MauiBiller.Core.Repositories;

public interface IWorkspaceRepository
{
    Task<Workspace> GetCurrentAsync(CancellationToken cancellationToken = default);
}

public interface IClientRepository
{
    Task<IReadOnlyList<Client>> ListAsync(CancellationToken cancellationToken = default);
    Task<Client?> GetByIdAsync(string clientId, CancellationToken cancellationToken = default);
    Task SaveAsync(Client client, CancellationToken cancellationToken = default);
    Task ArchiveAsync(string clientId, CancellationToken cancellationToken = default);
}

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> ListAsync(CancellationToken cancellationToken = default);
}

public interface IWorkItemRepository
{
    Task<IReadOnlyList<WorkItem>> ListAsync(CancellationToken cancellationToken = default);
}

public interface ITimeEntryRepository
{
    Task<IReadOnlyList<TimeEntry>> ListAsync(CancellationToken cancellationToken = default);
}

public interface IExpenseRepository
{
    Task<IReadOnlyList<Expense>> ListAsync(CancellationToken cancellationToken = default);
}

public interface IInvoiceDraftRepository
{
    Task<IReadOnlyList<InvoiceDraft>> ListAsync(CancellationToken cancellationToken = default);
}
