using MauiBiller.Configuration;
using MauiBiller.Core.Repositories;
using MauiBiller.Core.Services;
using MauiBiller.Infrastructure.Data;
using MauiBiller.Infrastructure.Services;
using MauiBiller.Pages.Authentication;
using MauiBiller.Pages.Billing;
using MauiBiller.Pages.Clients;
using MauiBiller.Pages.Projects;
using MauiBiller.Pages.Settings;
using MauiBiller.Pages.TimeTracking;
using MauiBiller.Pages.Workspace;
using MauiBiller.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MauiBiller.Extensions;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder RegisterAppConfiguration(this MauiAppBuilder builder)
    {
        var appConfiguration = AppConfigurationLoader.LoadCurrent();
        builder.Services.AddSingleton(appConfiguration);

        return builder;
    }

    public static MauiAppBuilder RegisterApplicationServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<InMemoryWorkspaceStore>();
        builder.Services.AddSingleton<IWorkspaceRepository, InMemoryWorkspaceRepository>();
        builder.Services.AddSingleton<IClientRepository, InMemoryClientRepository>();
        builder.Services.AddSingleton<IProjectRepository, InMemoryProjectRepository>();
        builder.Services.AddSingleton<IWorkItemRepository, InMemoryWorkItemRepository>();
        builder.Services.AddSingleton<ITimeEntryRepository, InMemoryTimeEntryRepository>();
        builder.Services.AddSingleton<IExpenseRepository, InMemoryExpenseRepository>();
        builder.Services.AddSingleton<IInvoiceDraftRepository, InMemoryInvoiceDraftRepository>();

        builder.Services.AddSingleton<ISecureValueStore, SecureValueStore>();
        builder.Services.AddSingleton<FirebaseAuthenticationService>();
        builder.Services.AddSingleton<IAuthenticationService>(serviceProvider => serviceProvider.GetRequiredService<FirebaseAuthenticationService>());
        builder.Services.AddSingleton<IAuthSessionService>(serviceProvider => serviceProvider.GetRequiredService<FirebaseAuthenticationService>());
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
        builder.Services.AddSingleton<IWorkspaceSnapshotService, WorkspaceSnapshotService>();

        builder.Services.AddSingleton<AppShell>();

        RegisterFeaturePage<LoginPage, LoginPageViewModel>(builder.Services);
        RegisterFeaturePage<RegisterPage, RegisterPageViewModel>(builder.Services);
        RegisterFeaturePage<ResetPasswordPage, ResetPasswordPageViewModel>(builder.Services);
        RegisterFeaturePage<InviteTeamMembersPage, InviteTeamMembersPageViewModel>(builder.Services);
        RegisterFeaturePage<ClientsPage, ClientsPageViewModel>(builder.Services);
        RegisterFeaturePage<ClientDetailsPage, ClientDetailsPageViewModel>(builder.Services);
        RegisterFeaturePage<ProjectsPage, ProjectsPageViewModel>(builder.Services);
        RegisterFeaturePage<ProjectWorkItemsPage, ProjectWorkItemsPageViewModel>(builder.Services);
        RegisterFeaturePage<TimerPage, TimerPageViewModel>(builder.Services);
        RegisterFeaturePage<AddManualTimePage, AddManualTimePageViewModel>(builder.Services);
        RegisterFeaturePage<BillingPage, BillingPageViewModel>(builder.Services);
        RegisterFeaturePage<CreateInvoicePage, CreateInvoicePageViewModel>(builder.Services);
        RegisterFeaturePage<SettingsPage, SettingsPageViewModel>(builder.Services);

        return builder;
    }

    private static void RegisterFeaturePage<TPage, TViewModel>(IServiceCollection services)
        where TPage : class
        where TViewModel : class
    {
        services.AddTransient<TViewModel>();
        services.AddTransient<TPage>();
    }
}
