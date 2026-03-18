using CommunityToolkit.Mvvm.Input;
using MauiBiller.Core.Models;
using MauiBiller.Core.Services;
using MauiBiller.Navigation;

namespace MauiBiller.ViewModels;

public abstract class AuthenticationViewModelBase : ViewModelBase
{
    private readonly IAuthenticationService authenticationService;
    private readonly INavigationService navigationService;
    private string errorMessage = string.Empty;
    private string statusMessage = string.Empty;

    protected AuthenticationViewModelBase(
        IAuthenticationService authenticationService,
        INavigationService navigationService,
        string pageTitle)
    {
        this.authenticationService = authenticationService;
        this.navigationService = navigationService;
        PageTitle = pageTitle;
    }

    public string ErrorMessage
    {
        get => errorMessage;
        protected set => SetProperty(ref errorMessage, value);
    }

    public string StatusMessage
    {
        get => statusMessage;
        protected set => SetProperty(ref statusMessage, value);
    }

    protected IAuthenticationService AuthenticationService => authenticationService;

    protected INavigationService NavigationService => navigationService;

    protected void ClearMessages()
    {
        ErrorMessage = string.Empty;
        StatusMessage = string.Empty;
    }

    protected void ShowError(string message)
    {
        StatusMessage = string.Empty;
        ErrorMessage = message;
    }

    protected void ShowStatus(string message)
    {
        ErrorMessage = string.Empty;
        StatusMessage = message;
    }

    protected async Task ExecuteAsync(Func<Task<AuthenticationResult>> action)
    {
        ClearMessages();
        IsBusy = true;

        try
        {
            var result = await action();

            if (!result.IsSuccessful)
            {
                ShowError(result.ErrorMessage ?? "The operation failed.");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed class LoginPageViewModel : AuthenticationViewModelBase
{
    private string email = string.Empty;
    private string password = string.Empty;

    public LoginPageViewModel(
        IAuthenticationService authenticationService,
        INavigationService navigationService)
        : base(authenticationService, navigationService, "Login")
    {
        SignInCommand = new AsyncRelayCommand(SignInAsync, () => !IsBusy);
        OpenRegisterCommand = new AsyncRelayCommand(() => NavigationService.GoToAsync(AppRoutes.Register));
        OpenResetPasswordCommand = new AsyncRelayCommand(() => NavigationService.GoToAsync(AppRoutes.ResetPassword));
        PropertyChanged += (_, eventArgs) =>
        {
            if (eventArgs.PropertyName is nameof(IsBusy))
            {
                SignInCommand.NotifyCanExecuteChanged();
            }
        };
    }

    public string Email
    {
        get => email;
        set => SetProperty(ref email, value);
    }

    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }

    public AsyncRelayCommand SignInCommand
    {
        get;
    }

    public IAsyncRelayCommand OpenRegisterCommand
    {
        get;
    }

    public IAsyncRelayCommand OpenResetPasswordCommand
    {
        get;
    }

    private async Task SignInAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await AuthenticationService.SignInAsync(Email, Password);

            if (result.IsSuccessful)
            {
                Password = string.Empty;
            }

            return result;
        });
    }
}

public sealed class RegisterPageViewModel : AuthenticationViewModelBase
{
    private string displayName = string.Empty;
    private string email = string.Empty;
    private string password = string.Empty;
    private string confirmPassword = string.Empty;

    public RegisterPageViewModel(
        IAuthenticationService authenticationService,
        INavigationService navigationService)
        : base(authenticationService, navigationService, "Register")
    {
        RegisterCommand = new AsyncRelayCommand(RegisterAsync, () => !IsBusy);
        OpenLoginCommand = new AsyncRelayCommand(() => NavigationService.GoToAsync(AppRoutes.Login));
        PropertyChanged += (_, eventArgs) =>
        {
            if (eventArgs.PropertyName is nameof(IsBusy))
            {
                RegisterCommand.NotifyCanExecuteChanged();
            }
        };
    }

    public string DisplayName
    {
        get => displayName;
        set => SetProperty(ref displayName, value);
    }

    public string Email
    {
        get => email;
        set => SetProperty(ref email, value);
    }

    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }

    public string ConfirmPassword
    {
        get => confirmPassword;
        set => SetProperty(ref confirmPassword, value);
    }

    public AsyncRelayCommand RegisterCommand
    {
        get;
    }

    public IAsyncRelayCommand OpenLoginCommand
    {
        get;
    }

    private async Task RegisterAsync()
    {
        if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
        {
            ShowError("Passwords do not match.");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var result = await AuthenticationService.RegisterAsync(DisplayName, Email, Password);

            if (result.IsSuccessful)
            {
                Password = string.Empty;
                ConfirmPassword = string.Empty;
            }

            return result;
        });
    }
}

public sealed class ResetPasswordPageViewModel : AuthenticationViewModelBase
{
    private string email = string.Empty;

    public ResetPasswordPageViewModel(
        IAuthenticationService authenticationService,
        INavigationService navigationService)
        : base(authenticationService, navigationService, "Reset Password")
    {
        SendResetCommand = new AsyncRelayCommand(SendResetAsync, () => !IsBusy);
        OpenLoginCommand = new AsyncRelayCommand(() => NavigationService.GoToAsync(AppRoutes.Login));
        PropertyChanged += (_, eventArgs) =>
        {
            if (eventArgs.PropertyName is nameof(IsBusy))
            {
                SendResetCommand.NotifyCanExecuteChanged();
            }
        };
    }

    public string Email
    {
        get => email;
        set => SetProperty(ref email, value);
    }

    public AsyncRelayCommand SendResetCommand
    {
        get;
    }

    public IAsyncRelayCommand OpenLoginCommand
    {
        get;
    }

    private async Task SendResetAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await AuthenticationService.SendPasswordResetAsync(Email);

            if (result.IsSuccessful)
            {
                ShowStatus("Password reset instructions have been sent.");
            }

            return result;
        });
    }
}
