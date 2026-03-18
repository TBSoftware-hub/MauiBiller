using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using MauiBiller.Core.Services;

namespace MauiBiller.ViewModels;

public abstract class FeaturePageViewModel : ViewModelBase
{
    private readonly INavigationService navigationService;
    private bool isInitialized;

    protected FeaturePageViewModel(INavigationService navigationService, string pageTitle)
    {
        this.navigationService = navigationService;
        PageTitle = pageTitle;
    }

    public ObservableCollection<FeatureMetricViewModel> Metrics
    {
        get;
    } = [];

    public ObservableCollection<FeatureActionViewModel> Actions
    {
        get;
    } = [];

    public string Summary
    {
        get;
        protected set;
    } = string.Empty;

    protected INavigationService NavigationService => navigationService;

    protected static FeatureMetricViewModel Metric(string label, string value, string detail)
    {
        return new FeatureMetricViewModel(label, value, detail);
    }

    protected static string FormatCurrency(decimal amount)
    {
        return amount.ToString("C");
    }

    protected static string FormatHours(TimeSpan duration)
    {
        return $"{duration.TotalHours:0.#} h";
    }

    public async Task EnsureInitializedAsync()
    {
        if (isInitialized)
        {
            return;
        }

        IsBusy = true;

        try
        {
            await LoadAsync();
            OnPropertyChanged(nameof(Summary));
            isInitialized = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected abstract Task LoadAsync();

    protected FeatureActionViewModel CreateNavigationAction(string title, string description, string route)
    {
        return new FeatureActionViewModel(
            title,
            description,
            new AsyncRelayCommand(() => NavigationService.GoToAsync(route)));
    }

    protected void ReplaceMetrics(params IEnumerable<FeatureMetricViewModel>[] metricSets)
    {
        Metrics.Clear();

        foreach (var metric in metricSets.SelectMany(set => set))
        {
            Metrics.Add(metric);
        }
    }

    protected void ReplaceActions(params IEnumerable<FeatureActionViewModel>[] actionSets)
    {
        Actions.Clear();

        foreach (var action in actionSets.SelectMany(set => set))
        {
            Actions.Add(action);
        }
    }
}
