using MauiBiller.Core.Models;

namespace MauiBiller.Core.Services;

public interface INavigationService
{
    Task GoToAsync(string route);
}

public interface IWorkspaceSnapshotService
{
    Task<WorkspaceSnapshot> GetCurrentSnapshotAsync(CancellationToken cancellationToken = default);
}

public interface IWorkspaceBootstrapService
{
    Task<WorkspaceBootstrapResult> EnsureWorkspaceReadyAsync(CancellationToken cancellationToken = default);
}
