using MauiBiller.ViewModels;

namespace MauiBiller.Pages.Authentication;

public sealed class ResetPasswordPage : ContentPage
{
    public ResetPasswordPage(ResetPasswordPageViewModel viewModel)
    {
        BindingContext = viewModel;
        SetBinding(TitleProperty, new Binding(nameof(ResetPasswordPageViewModel.PageTitle)));

        var titleLabel = new Label
        {
            FontAttributes = FontAttributes.Bold,
            FontSize = 28,
            Text = "Reset password"
        };

        var subtitleLabel = new Label
        {
            Text = "Request a Firebase Auth password-reset email for your workspace account.",
            LineBreakMode = LineBreakMode.WordWrap
        };

        var emailEntry = CreateEntry("Email address", Keyboard.Email);
        emailEntry.SetBinding(Entry.TextProperty, nameof(ResetPasswordPageViewModel.Email), BindingMode.TwoWay);

        var errorLabel = CreateMessageLabel(Colors.Firebrick);
        errorLabel.SetBinding(Label.TextProperty, nameof(ResetPasswordPageViewModel.ErrorMessage));

        var statusLabel = CreateMessageLabel(Colors.SeaGreen);
        statusLabel.SetBinding(Label.TextProperty, nameof(ResetPasswordPageViewModel.StatusMessage));

        var resetButton = new Button
        {
            Text = "Send reset instructions"
        };
        resetButton.SetBinding(Button.CommandProperty, nameof(ResetPasswordPageViewModel.SendResetCommand));

        var loginButton = new Button
        {
            Text = "Back to login"
        };
        loginButton.SetBinding(Button.CommandProperty, nameof(ResetPasswordPageViewModel.OpenLoginCommand));

        var activityIndicator = new ActivityIndicator
        {
            HorizontalOptions = LayoutOptions.Start
        };
        activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(ResetPasswordPageViewModel.IsBusy));
        activityIndicator.SetBinding(ActivityIndicator.IsVisibleProperty, nameof(ResetPasswordPageViewModel.IsBusy));

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
                    errorLabel,
                    statusLabel,
                    activityIndicator,
                    resetButton,
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
