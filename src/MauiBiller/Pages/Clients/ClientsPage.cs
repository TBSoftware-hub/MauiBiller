using MauiBiller.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace MauiBiller.Pages.Clients;

public sealed class ClientsPage : ContentPage
{
    private readonly ClientsPageViewModel viewModel;

    public ClientsPage(ClientsPageViewModel viewModel)
    {
        this.viewModel = viewModel;
        BindingContext = viewModel;
        SetBinding(TitleProperty, new Binding(nameof(ClientsPageViewModel.PageTitle)));

        var toolbarItem = new ToolbarItem
        {
            Text = "Add Client"
        };
        toolbarItem.SetBinding(MenuItem.CommandProperty, nameof(ClientsPageViewModel.OpenNewClientCommand));
        ToolbarItems.Add(toolbarItem);

        var titleLabel = new Label
        {
            FontAttributes = FontAttributes.Bold,
            FontSize = 28
        };
        titleLabel.SetBinding(Label.TextProperty, nameof(ClientsPageViewModel.PageTitle));

        var summaryLabel = new Label
        {
            LineBreakMode = LineBreakMode.WordWrap
        };
        summaryLabel.SetBinding(Label.TextProperty, nameof(ClientsPageViewModel.Summary));

        var errorLabel = new Label
        {
            TextColor = Color.FromArgb("#B91C1C"),
            LineBreakMode = LineBreakMode.WordWrap
        };
        errorLabel.SetBinding(Label.TextProperty, nameof(ClientsPageViewModel.ErrorMessage));

        var activityIndicator = new ActivityIndicator
        {
            HorizontalOptions = LayoutOptions.Start
        };
        activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(ClientsPageViewModel.IsBusy));
        activityIndicator.SetBinding(ActivityIndicator.IsVisibleProperty, nameof(ClientsPageViewModel.IsBusy));

        var clientsView = new CollectionView
        {
            SelectionMode = SelectionMode.None,
            EmptyView = new Label
            {
                Text = "No active clients yet.",
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 24)
            },
            ItemTemplate = new DataTemplate(() =>
            {
                var nameLabel = new Label
                {
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 18
                };
                nameLabel.SetBinding(Label.TextProperty, nameof(ClientListItemViewModel.Name));

                var contactLabel = new Label();
                contactLabel.SetBinding(Label.TextProperty, nameof(ClientListItemViewModel.ContactName));

                var emailLabel = new Label();
                emailLabel.SetBinding(Label.TextProperty, nameof(ClientListItemViewModel.ContactEmail));

                var projectsLabel = new Label
                {
                    TextColor = Color.FromArgb("#475569")
                };
                projectsLabel.SetBinding(Label.TextProperty, nameof(ClientListItemViewModel.ActiveProjectSummary));

                var openButton = new Button
                {
                    Text = "Open Client"
                };
                openButton.SetBinding(Button.CommandProperty, nameof(ClientListItemViewModel.OpenCommand));

                return new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb("#F8FAFC")),
                    Stroke = new SolidColorBrush(Color.FromArgb("#E2E8F0")),
                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(16)
                    },
                    Padding = 16,
                    Margin = new Thickness(0, 0, 0, 12),
                    Content = new VerticalStackLayout
                    {
                        Spacing = 8,
                        Children =
                        {
                            nameLabel,
                            contactLabel,
                            emailLabel,
                            projectsLabel,
                            openButton
                        }
                    }
                };
            })
        };
        clientsView.SetBinding(ItemsView.ItemsSourceProperty, nameof(ClientsPageViewModel.Clients));

        var refreshView = new RefreshView
        {
            Content = clientsView
        };
        refreshView.SetBinding(RefreshView.CommandProperty, nameof(ClientsPageViewModel.RefreshCommand));
        refreshView.SetBinding(RefreshView.IsRefreshingProperty, new Binding(nameof(ClientsPageViewModel.IsBusy), mode: BindingMode.OneWay));

        Content = new Grid
        {
            Padding = 24,
            Children =
            {
                new Grid
                {
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Star }
                    },
                    RowSpacing = 16,
                    Children =
                    {
                        titleLabel,
                        summaryLabel,
                        errorLabel,
                        activityIndicator,
                        refreshView
                    }
                }
            }
        };

        Grid.SetRow(summaryLabel, 1);
        Grid.SetRow(errorLabel, 2);
        Grid.SetRow(activityIndicator, 3);
        Grid.SetRow(refreshView, 4);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await viewModel.RefreshAsync();
        }
        catch (Exception exception)
        {
            viewModel.ShowUnexpectedError(exception.Message);
        }
    }
}
