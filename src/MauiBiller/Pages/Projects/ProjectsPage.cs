using MauiBiller.Navigation;
using MauiBiller.Pages.Shared;

namespace MauiBiller.Pages.Projects;

public sealed class ProjectsPage : AppPlaceholderPage
{
    public ProjectsPage()
        : base(
            "Projects",
            "Organize billable work beneath each client and define the project structure used by time tracking and billing.",
            CreateAction("Open Project Work Items", AppRoutes.ProjectWorkItems))
    {
    }
}
