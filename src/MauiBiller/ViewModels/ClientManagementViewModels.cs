using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.Input;
using MauiBiller.Core.Models;
using MauiBiller.Core.Repositories;
using MauiBiller.Core.Services;
using MauiBiller.Navigation;

namespace MauiBiller.ViewModels;

public sealed class ClientsPageViewModel : ViewModelBase
{
    private readonly IClientRepository clientRepository;
    private readonly INavigationService navigationService;
    private readonly IProjectRepository projectRepository;
    private string errorMessage = string.Empty;
    private string summary = string.Empty;

    public ClientsPageViewModel(
        IClientRepository clientRepository,
        IProjectRepository projectRepository,
        INavigationService navigationService)
    {
        this.clientRepository = clientRepository;
        this.projectRepository = projectRepository;
        this.navigationService = navigationService;
        PageTitle = "Clients";
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        OpenNewClientCommand = new AsyncRelayCommand(() => navigationService.GoToAsync(CreateClientRoute(null)));
    }

    public ObservableCollection<ClientListItemViewModel> Clients
    {
        get;
    } = [];

    public string Summary
    {
        get => summary;
        private set => SetProperty(ref summary, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public IAsyncRelayCommand RefreshCommand
    {
        get;
    }

    public IAsyncRelayCommand OpenNewClientCommand
    {
        get;
    }

    public async Task RefreshAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            var clientsTask = clientRepository.ListAsync();
            var projectsTask = projectRepository.ListAsync();

            await Task.WhenAll(clientsTask, projectsTask);

            var clients = await clientsTask;
            var projects = await projectsTask;
            var activeClients = clients
                .Where(client => !client.IsArchived)
                .OrderBy(client => client.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            var archivedCount = clients.Count(client => client.IsArchived);

            Clients.Clear();

            foreach (var client in activeClients)
            {
                var projectCount = projects.Count(project => project.ClientId == client.Id && !project.IsArchived);
                Clients.Add(new ClientListItemViewModel(
                    client.Id,
                    client.Name,
                    client.ContactName,
                    client.ContactEmail,
                    projectCount,
                    new AsyncRelayCommand(() => navigationService.GoToAsync(CreateClientRoute(client.Id)))));
            }

            Summary = activeClients.Count is 0
                ? "No active clients are saved locally yet. Add your first client to start organizing billable work."
                : $"{activeClients.Count} active client(s) loaded from local storage. {archivedCount} archived client(s) remain available for later management.";
        }
        catch (Exception exception)
        {
            Summary = "We couldn't load clients from local storage.";
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void ShowUnexpectedError(string message)
    {
        ErrorMessage = message;
    }

    private static string CreateClientRoute(string? clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            return $"{AppRoutes.ClientDetails}?{AppRoutes.ClientModeQueryParameter}={AppRoutes.ClientModeNew}";
        }

        return $"{AppRoutes.ClientDetails}?{AppRoutes.ClientIdQueryParameter}={Uri.EscapeDataString(clientId)}";
    }
}

public sealed class ClientDetailsPageViewModel : ViewModelBase
{
    private static readonly EmailAddressAttribute EmailValidator = new();
    private readonly IClientRepository clientRepository;
    private readonly INavigationService navigationService;
    private readonly IProjectRepository projectRepository;
    private string activeProjectSummary = string.Empty;
    private string contactEmail = string.Empty;
    private string contactName = string.Empty;
    private string currentClientId = string.Empty;
    private string errorMessage = string.Empty;
    private string name = string.Empty;
    private string summary = string.Empty;

    public ClientDetailsPageViewModel(
        IClientRepository clientRepository,
        IProjectRepository projectRepository,
        INavigationService navigationService)
    {
        this.clientRepository = clientRepository;
        this.projectRepository = projectRepository;
        this.navigationService = navigationService;
        PageTitle = "Client Details";
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        ArchiveCommand = new AsyncRelayCommand(ArchiveAsync, () => CanArchive);
        CancelCommand = new AsyncRelayCommand(() => navigationService.GoToAsync(AppRoutes.Clients));
    }

    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    public string ContactName
    {
        get => contactName;
        set => SetProperty(ref contactName, value);
    }

    public string ContactEmail
    {
        get => contactEmail;
        set => SetProperty(ref contactEmail, value);
    }

    public string Summary
    {
        get => summary;
        private set => SetProperty(ref summary, value);
    }

    public string ActiveProjectSummary
    {
        get => activeProjectSummary;
        private set => SetProperty(ref activeProjectSummary, value);
    }

    public string ErrorMessage
    {
        get => errorMessage;
        private set => SetProperty(ref errorMessage, value);
    }

    public bool CanArchive => !string.IsNullOrWhiteSpace(currentClientId);

    public IAsyncRelayCommand SaveCommand
    {
        get;
    }

    public IAsyncRelayCommand ArchiveCommand
    {
        get;
    }

    public IAsyncRelayCommand CancelCommand
    {
        get;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        currentClientId = GetQueryValue(query, AppRoutes.ClientIdQueryParameter);
        var mode = GetQueryValue(query, AppRoutes.ClientModeQueryParameter);

        if (string.Equals(mode, AppRoutes.ClientModeNew, StringComparison.OrdinalIgnoreCase))
        {
            currentClientId = string.Empty;
        }

        OnPropertyChanged(nameof(CanArchive));
        ArchiveCommand.NotifyCanExecuteChanged();
    }

    public async Task RefreshAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            if (string.IsNullOrWhiteSpace(currentClientId))
            {
                PageTitle = "New Client";
                Summary = "Create a client with the billing contact details you need for future invoices.";
                ActiveProjectSummary = "New clients can be linked to projects after you save them.";
                Name = string.Empty;
                ContactName = string.Empty;
                ContactEmail = string.Empty;
                return;
            }

            var client = await clientRepository.GetByIdAsync(currentClientId)
                ?? throw new InvalidOperationException($"Client '{currentClientId}' was not found.");
            var projects = await projectRepository.ListAsync();
            var activeProjectCount = projects.Count(project => project.ClientId == client.Id && !project.IsArchived);

            PageTitle = client.IsArchived ? "Archived Client" : "Client Details";
            Summary = "Update the saved client metadata used by billing and future project setup flows.";
            ActiveProjectSummary = activeProjectCount is 1
                ? "1 active project is linked to this client."
                : $"{activeProjectCount} active projects are linked to this client.";
            Name = client.Name;
            ContactName = client.ContactName;
            ContactEmail = client.ContactEmail;
        }
        catch (Exception exception)
        {
            Summary = "We couldn't load this client right now.";
            ActiveProjectSummary = string.Empty;
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveAsync()
    {
        ErrorMessage = Validate();

        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            return;
        }

        IsBusy = true;

        try
        {
            var clientId = string.IsNullOrWhiteSpace(currentClientId)
                ? Guid.NewGuid().ToString("N")
                : currentClientId;
            var existingClient = string.IsNullOrWhiteSpace(currentClientId)
                ? null
                : await clientRepository.GetByIdAsync(currentClientId);
            var client = new Client(
                clientId,
                Name.Trim(),
                ContactName.Trim(),
                ContactEmail.Trim(),
                existingClient?.IsArchived ?? false);

            await clientRepository.SaveAsync(client);
            await navigationService.GoToAsync(AppRoutes.Clients);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ArchiveAsync()
    {
        if (!CanArchive)
        {
            return;
        }

        IsBusy = true;

        try
        {
            await clientRepository.ArchiveAsync(currentClientId);
            await navigationService.GoToAsync(AppRoutes.Clients);
        }
        catch (Exception exception)
        {
            ErrorMessage = exception.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void ShowUnexpectedError(string message)
    {
        ErrorMessage = message;
    }

    private string Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return "Client name is required.";
        }

        if (string.IsNullOrWhiteSpace(ContactName))
        {
            return "Contact name is required.";
        }

        if (string.IsNullOrWhiteSpace(ContactEmail))
        {
            return "Contact email is required.";
        }

        if (!EmailValidator.IsValid(ContactEmail.Trim()))
        {
            return "Enter a valid contact email address.";
        }

        return string.Empty;
    }

    private static string GetQueryValue(IDictionary<string, object> query, string key)
    {
        if (!query.TryGetValue(key, out var value))
        {
            return string.Empty;
        }

        return value switch
        {
            string text => text,
            _ => value.ToString() ?? string.Empty
        };
    }
}

public sealed class ClientListItemViewModel(
    string clientId,
    string name,
    string contactName,
    string contactEmail,
    int activeProjectCount,
    IAsyncRelayCommand openCommand)
{
    public string ClientId => clientId;

    public string Name => name;

    public string ContactName => contactName;

    public string ContactEmail => contactEmail;

    public string ActiveProjectSummary => activeProjectCount is 1
        ? "1 active project"
        : $"{activeProjectCount} active projects";

    public IAsyncRelayCommand OpenCommand => openCommand;
}
