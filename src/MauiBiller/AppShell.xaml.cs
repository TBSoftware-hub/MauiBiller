using MauiBiller.Configuration;
using MauiBiller.Core.Models;
using MauiBiller.Core.Services;
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
    private readonly IAuthenticationService authenticationService;
    private readonly IAuthSessionService authSessionService;
    private readonly IServiceProvider serviceProvider;
    private bool hasInitializedSession;

    public AppShell(
        AppConfiguration appConfiguration,
        IServiceProvider serviceProvider,
        IAuthenticationService authenticationService,
        IAuthSessionService authSessionService)
    {
        InitializeComponent();

        this.authenticationService = authenticationService;
        this.authSessionService = authSessionService;
        this.serviceProvider = serviceProvider;
        BindingContext = appConfiguration;
        RegisterRoutes();
        authSessionService.SessionChanged += OnSessionChanged;
        ApplyShellState();
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

    private void ApplyShellState()
    {
        Items.Clear();
        ToolbarItems.Clear();

        if (authSessionService.IsAuthenticated)
        {
            Items.Add(CreateFlyoutItem("Clients", AppRoutes.Clients, typeof(ClientsPage)));
            Items.Add(CreateFlyoutItem("Projects", AppRoutes.Projects, typeof(ProjectsPage)));
            Items.Add(CreateFlyoutItem("Timer", AppRoutes.Timer, typeof(TimerPage)));
            Items.Add(CreateFlyoutItem("Billing", AppRoutes.Billing, typeof(BillingPage)));
            Items.Add(CreateFlyoutItem("Settings", AppRoutes.Settings, typeof(SettingsPage)));
            ToolbarItems.Add(new ToolbarItem("Sign Out", null, async () => await authenticationService.SignOutAsync()));
            CurrentItem = Items.FirstOrDefault();
            return;
        }

        Items.Add(CreateFlyoutItem("Login", AppRoutes.Login, typeof(LoginPage)));
        CurrentItem = Items.FirstOrDefault();
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

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (hasInitializedSession)
        {
            return;
        }

        hasInitializedSession = true;
        await authSessionService.InitializeAsync();
        await GoToAsync(AppRoutes.AsRoot(authSessionService.IsAuthenticated ? AppRoutes.Clients : AppRoutes.Login));
    }

    private void OnSessionChanged(object? sender, AuthSessionChangedEventArgs eventArgs)
    {
        Dispatcher.Dispatch(async () =>
        {
            ApplyShellState();
            await GoToAsync(AppRoutes.AsRoot(eventArgs.IsAuthenticated ? AppRoutes.Clients : AppRoutes.Login));
        });
    }
}
