using MauiBiller.ViewModels;

namespace MauiBiller.Pages.Authentication;

public sealed class LoginPage : ContentPage
{
    public LoginPage(LoginPageViewModel viewModel)
    {
        BindingContext = viewModel;
        SetBinding(TitleProperty, new Binding(nameof(LoginPageViewModel.PageTitle)));

        var titleLabel = new Label
        {
            FontAttributes = FontAttributes.Bold,
            FontSize = 28,
            Text = "Welcome back"
        };

        var subtitleLabel = new Label
        {
            Text = "Sign in with your Firebase-backed workspace account to unlock clients, projects, billing, and time tracking.",
            LineBreakMode = LineBreakMode.WordWrap
        };

        var emailEntry = CreateEntry("Email address", Keyboard.Email);
        emailEntry.SetBinding(Entry.TextProperty, nameof(LoginPageViewModel.Email), BindingMode.TwoWay);

        var passwordEntry = CreateEntry("Password");
        passwordEntry.IsPassword = true;
        passwordEntry.SetBinding(Entry.TextProperty, nameof(LoginPageViewModel.Password), BindingMode.TwoWay);

        var errorLabel = CreateMessageLabel(Colors.Firebrick);
        errorLabel.SetBinding(Label.TextProperty, nameof(LoginPageViewModel.ErrorMessage));

        var statusLabel = CreateMessageLabel(Colors.SeaGreen);
        statusLabel.SetBinding(Label.TextProperty, nameof(LoginPageViewModel.StatusMessage));

        var signInButton = new Button
        {
            Text = "Sign In"
        };
        signInButton.SetBinding(Button.CommandProperty, nameof(LoginPageViewModel.SignInCommand));

        var registerButton = new Button
        {
            Text = "Create account"
        };
        registerButton.SetBinding(Button.CommandProperty, nameof(LoginPageViewModel.OpenRegisterCommand));

        var resetPasswordButton = new Button
        {
            Text = "Forgot password?"
        };
        resetPasswordButton.SetBinding(Button.CommandProperty, nameof(LoginPageViewModel.OpenResetPasswordCommand));

        var activityIndicator = new ActivityIndicator
        {
            HorizontalOptions = LayoutOptions.Start
        };
        activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(LoginPageViewModel.IsBusy));
        activityIndicator.SetBinding(ActivityIndicator.IsVisibleProperty, nameof(LoginPageViewModel.IsBusy));

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 16,
                Children =
                {
                    titleLabel,
                    subtitleLabel,
                    emailEntry,
                    passwordEntry,
                    errorLabel,
                    statusLabel,
                    activityIndicator,
                    signInButton,
                    registerButton,
                    resetPasswordButton
                }
            }
        };
    }

    private static Entry CreateEntry(string placeholder, Keyboard? keyboard = null)
    {
        return new Entry
        {
            Placeholder = placeholder,
            Keyboard = keyboard ?? Keyboard.Default,
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing
        };
    }

    private static Label CreateMessageLabel(Color textColor)
    {
        return new Label
        {
            TextColor = textColor,
            LineBreakMode = LineBreakMode.WordWrap
        };
    }
}
