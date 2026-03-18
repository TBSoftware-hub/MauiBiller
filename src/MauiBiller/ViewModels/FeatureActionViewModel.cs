using CommunityToolkit.Mvvm.Input;

namespace MauiBiller.ViewModels;

public sealed class FeatureActionViewModel(string title, string description, IAsyncRelayCommand command)
{
    public string Title
    {
        get;
    } = title;

    public string Description
    {
        get;
    } = description;

    public IAsyncRelayCommand Command
    {
        get;
    } = command;
}
