using MauiBiller.Configuration;
using MauiBiller.Navigation;
using MauiBiller.Pages.Authentication;
using MauiBiller.Pages.Billing;
using MauiBiller.Pages.Clients;
using MauiBiller.Pages.Projects;
using MauiBiller.Pages.TimeTracking;
using MauiBiller.Pages.Workspace;

namespace MauiBiller;

public partial class AppShell : Shell
{
    public AppShell(AppConfiguration appConfiguration)
    {
        InitializeComponent();

        BindingContext = appConfiguration;
        RegisterRoutes();
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(AppRoutes.Register, typeof(RegisterPage));
        Routing.RegisterRoute(AppRoutes.ResetPassword, typeof(ResetPasswordPage));
        Routing.RegisterRoute(AppRoutes.InviteTeamMembers, typeof(InviteTeamMembersPage));
        Routing.RegisterRoute(AppRoutes.ClientDetails, typeof(ClientDetailsPage));
        Routing.RegisterRoute(AppRoutes.ProjectWorkItems, typeof(ProjectWorkItemsPage));
        Routing.RegisterRoute(AppRoutes.AddManualTime, typeof(AddManualTimePage));
        Routing.RegisterRoute(AppRoutes.CreateInvoice, typeof(CreateInvoicePage));
    }
}
