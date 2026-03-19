namespace MauiBiller.Navigation;

public static class AppRoutes
{
    public const string ClientIdQueryParameter = "clientId";
    public const string ClientModeQueryParameter = "mode";
    public const string ClientModeNew = "new";
    public const string Login = "login";
    public const string Clients = "clients";
    public const string Projects = "projects";
    public const string Timer = "timer";
    public const string Billing = "billing";
    public const string Settings = "settings";
    public const string Register = "register";
    public const string ResetPassword = "reset-password";
    public const string InviteTeamMembers = "invite-team-members";
    public const string ClientDetails = "client-details";
    public const string ProjectWorkItems = "project-work-items";
    public const string AddManualTime = "add-manual-time";
    public const string CreateInvoice = "create-invoice";

    public static bool IsAuthenticationRoute(string route)
    {
        return route is Login or Register or ResetPassword;
    }

    public static bool IsTopLevelRoute(string route)
    {
        return route is Login or Clients or Projects or Timer or Billing or Settings;
    }

    public static string AsRoot(string route)
    {
        return $"//{route}";
    }
}
