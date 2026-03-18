using MauiBiller.ViewModels;

namespace MauiBiller.Pages.Authentication;

public sealed class RegisterPage : ContentPage
{
    public RegisterPage(RegisterPageViewModel viewModel)
    {
        BindingContext = viewModel;
        SetBinding(TitleProperty, new Binding(nameof(RegisterPageViewModel.PageTitle)));

        var titleLabel = new Label
        {
            FontAttributes = FontAttributes.Bold,
            FontSize = 28,
            Text = "Create your workspace owner account"
        };

        var subtitleLabel = new Label
        {
            Text = "Register with Firebase Auth to start a secured session and restore it automatically across app launches.",
            LineBreakMode = LineBreakMode.WordWrap
        };

        var nameEntry = CreateEntry("Full name");
        nameEntry.SetBinding(Entry.TextProperty, nameof(RegisterPageViewModel.DisplayName), BindingMode.TwoWay);

        var emailEntry = CreateEntry("Email address", Keyboard.Email);
        emailEntry.SetBinding(Entry.TextProperty, nameof(RegisterPageViewModel.Email), BindingMode.TwoWay);

        var passwordEntry = CreateEntry("Password");
        passwordEntry.IsPassword = true;
        passwordEntry.SetBinding(Entry.TextProperty, nameof(RegisterPageViewModel.Password), BindingMode.TwoWay);

        var confirmPasswordEntry = CreateEntry("Confirm password");
        confirmPasswordEntry.IsPassword = true;
        confirmPasswordEntry.SetBinding(Entry.TextProperty, nameof(RegisterPageViewModel.ConfirmPassword), BindingMode.TwoWay);

        var errorLabel = CreateMessageLabel(Colors.Firebrick);
        errorLabel.SetBinding(Label.TextProperty, nameof(RegisterPageViewModel.ErrorMessage));

        var statusLabel = CreateMessageLabel(Colors.SeaGreen);
        statusLabel.SetBinding(Label.TextProperty, nameof(RegisterPageViewModel.StatusMessage));

        var registerButton = new Button
        {
            Text = "Create account"
        };
        registerButton.SetBinding(Button.CommandProperty, nameof(RegisterPageViewModel.RegisterCommand));

        var loginButton = new Button
        {
            Text = "Back to login"
        };
        loginButton.SetBinding(Button.CommandProperty, nameof(RegisterPageViewModel.OpenLoginCommand));

        var activityIndicator = new ActivityIndicator
        {
            HorizontalOptions = LayoutOptions.Start
        };
        activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(RegisterPageViewModel.IsBusy));
        activityIndicator.SetBinding(ActivityIndicator.IsVisibleProperty, nameof(RegisterPageViewModel.IsBusy));

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
                    nameEntry,
                    emailEntry,
                    passwordEntry,
                    confirmPasswordEntry,
                    errorLabel,
                    statusLabel,
                    activityIndicator,
                    registerButton,
                    loginButton
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
