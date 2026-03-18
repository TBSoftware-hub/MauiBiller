using MauiBiller.Core.Models;
using MauiBiller.Core.Repositories;
using MauiBiller.Core.Services;

namespace MauiBiller.Infrastructure.Services;

public sealed class WorkspaceSnapshotService(
    IWorkspaceRepository workspaceRepository,
    IClientRepository clientRepository,
    IProjectRepository projectRepository,
    IWorkItemRepository workItemRepository,
    ITimeEntryRepository timeEntryRepository,
    IExpenseRepository expenseRepository,
    IInvoiceDraftRepository invoiceDraftRepository) : IWorkspaceSnapshotService
{
    public async Task<WorkspaceSnapshot> GetCurrentSnapshotAsync(CancellationToken cancellationToken = default)
    {
        var workspaceTask = workspaceRepository.GetCurrentAsync(cancellationToken);
        var clientsTask = clientRepository.ListAsync(cancellationToken);
        var projectsTask = projectRepository.ListAsync(cancellationToken);
        var workItemsTask = workItemRepository.ListAsync(cancellationToken);
        var timeEntriesTask = timeEntryRepository.ListAsync(cancellationToken);
        var expensesTask = expenseRepository.ListAsync(cancellationToken);
        var invoiceDraftsTask = invoiceDraftRepository.ListAsync(cancellationToken);

        await Task.WhenAll(
            workspaceTask,
            clientsTask,
            projectsTask,
            workItemsTask,
            timeEntriesTask,
            expensesTask,
            invoiceDraftsTask);

        return new WorkspaceSnapshot(
            await workspaceTask,
            await clientsTask,
            await projectsTask,
            await workItemsTask,
            await timeEntriesTask,
            await expensesTask,
            await invoiceDraftsTask);
    }
}
