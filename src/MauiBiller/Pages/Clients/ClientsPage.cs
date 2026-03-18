using MauiBiller.Navigation;
using MauiBiller.Pages.Shared;

namespace MauiBiller.Pages.Clients;

public sealed class ClientsPage : AppPlaceholderPage
{
    public ClientsPage()
        : base(
            "Clients",
            "Browse and manage client records, billing metadata, and the primary entry point into client-specific details.",
            CreateAction("Open Client Details", AppRoutes.ClientDetails),
            CreateAction("Invite Team Members", AppRoutes.InviteTeamMembers))
    {
    }
}
