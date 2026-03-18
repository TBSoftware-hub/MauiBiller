using MauiBiller.Pages.Shared;

namespace MauiBiller.Pages.TimeTracking;

public sealed class AddManualTimePage : AppPlaceholderPage
{
    public AddManualTimePage()
        : base(
            "Add Manual Time",
            "Capture a manual time entry with the date, duration, notes, and billable context needed for later invoicing.")
    {
    }
}
