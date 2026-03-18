using CommunityToolkit.Mvvm.ComponentModel;

namespace MauiBiller.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    private bool isBusy;
    private string pageTitle = string.Empty;

    public bool IsBusy
    {
        get => isBusy;
        protected set => SetProperty(ref isBusy, value);
    }

    public string PageTitle
    {
        get => pageTitle;
        protected set => SetProperty(ref pageTitle, value);
    }
}
