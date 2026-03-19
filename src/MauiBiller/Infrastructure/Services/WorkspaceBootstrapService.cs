using MauiBiller.Core.Models;
using MauiBiller.Core.Services;
using MauiBiller.Infrastructure.Data;

namespace MauiBiller.Infrastructure.Services;

public sealed class WorkspaceBootstrapService(LocalWorkspaceStore localWorkspaceStore) : IWorkspaceBootstrapService
{
    public Task<WorkspaceBootstrapResult> EnsureWorkspaceReadyAsync(CancellationToken cancellationToken = default)
    {
        return localWorkspaceStore.EnsureOwnerWorkspaceAsync(cancellationToken);
    }
}
