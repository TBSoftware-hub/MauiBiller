using MauiBiller.Navigation;
using MauiBiller.Pages.Shared;

namespace MauiBiller.Pages.TimeTracking;

public sealed class TimerPage : AppPlaceholderPage
{
    public TimerPage()
        : base(
            "Timer",
            "Track live work against the selected client, project, and work item while preserving a clean offline-first flow.",
            CreateAction("Open Add Manual Time", AppRoutes.AddManualTime))
    {
    }
}
