using MauiBiller.Configuration;
using MauiBiller.Navigation;
using MauiBiller.Pages.Authentication;
using MauiBiller.Pages.Billing;
using MauiBiller.Pages.Clients;
using MauiBiller.Pages.Projects;
using MauiBiller.Pages.Settings;
using MauiBiller.Pages.TimeTracking;
using MauiBiller.Pages.Workspace;
using Microsoft.Extensions.DependencyInjection;

namespace MauiBiller;

public partial class AppShell : Shell
{
    private readonly IServiceProvider serviceProvider;

    public AppShell(AppConfiguration appConfiguration, IServiceProvider serviceProvider)
    {
        InitializeComponent();

        this.serviceProvider = serviceProvider;
        BindingContext = appConfiguration;
        RegisterRoutes();
        BuildFlyout();
    }

    private void RegisterRoutes()
    {
        Routing.RegisterRoute(AppRoutes.Register, new ServiceRouteFactory<RegisterPage>(serviceProvider));
        Routing.RegisterRoute(AppRoutes.ResetPassword, new ServiceRouteFactory<ResetPasswordPage>(serviceProvider));
        Routing.RegisterRoute(AppRoutes.InviteTeamMembers, new ServiceRouteFactory<InviteTeamMembersPage>(serviceProvider));
        Routing.RegisterRoute(AppRoutes.ClientDetails, new ServiceRouteFactory<ClientDetailsPage>(serviceProvider));
        Routing.RegisterRoute(AppRoutes.ProjectWorkItems, new ServiceRouteFactory<ProjectWorkItemsPage>(serviceProvider));
        Routing.RegisterRoute(AppRoutes.AddManualTime, new ServiceRouteFactory<AddManualTimePage>(serviceProvider));
        Routing.RegisterRoute(AppRoutes.CreateInvoice, new ServiceRouteFactory<CreateInvoicePage>(serviceProvider));
    }

    private void BuildFlyout()
    {
        Items.Add(CreateFlyoutItem("Login", AppRoutes.Login, typeof(LoginPage)));
        Items.Add(CreateFlyoutItem("Clients", AppRoutes.Clients, typeof(ClientsPage)));
        Items.Add(CreateFlyoutItem("Projects", AppRoutes.Projects, typeof(ProjectsPage)));
        Items.Add(CreateFlyoutItem("Timer", AppRoutes.Timer, typeof(TimerPage)));
        Items.Add(CreateFlyoutItem("Billing", AppRoutes.Billing, typeof(BillingPage)));
        Items.Add(CreateFlyoutItem("Settings", AppRoutes.Settings, typeof(SettingsPage)));
    }

    private FlyoutItem CreateFlyoutItem(string title, string route, Type pageType)
    {
        return new FlyoutItem
        {
            Title = title,
            Route = route,
            Items =
            {
                new ShellContent
                {
                    Title = title,
                    Route = route,
                    ContentTemplate = new DataTemplate(() => (Page)serviceProvider.GetRequiredService(pageType))
                }
            }
        };
    }
}
