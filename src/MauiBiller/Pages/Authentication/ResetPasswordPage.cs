using MauiBiller.Pages.Shared;

namespace MauiBiller.Pages.Authentication;

public sealed class ResetPasswordPage : AppPlaceholderPage
{
    public ResetPasswordPage()
        : base(
            "Reset Password",
            "Provide a recovery path that hands off to Firebase Auth and returns the user to a valid login state.")
    {
    }
}
