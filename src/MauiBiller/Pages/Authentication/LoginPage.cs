using MauiBiller.Navigation;
using MauiBiller.Pages.Shared;

namespace MauiBiller.Pages.Authentication;

public sealed class LoginPage : AppPlaceholderPage
{
    public LoginPage()
        : base(
            "Login",
            "Sign in to a MauiBiller workspace with the Firebase Auth-backed experience and move into the authenticated shell.",
            CreateAction("Open Register", AppRoutes.Register),
            CreateAction("Open Reset Password", AppRoutes.ResetPassword))
    {
    }
}
