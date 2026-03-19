using MauiBiller.Navigation;
using MauiBiller.ViewModels;
using Microsoft.Maui.Controls.Shapes;

namespace MauiBiller.Pages.Clients;

public sealed class ClientDetailsPage : ContentPage, IQueryAttributable
{
    private readonly ClientDetailsPageViewModel viewModel;

    public ClientDetailsPage(ClientDetailsPageViewModel viewModel)
    {
        this.viewModel = viewModel;
        BindingContext = viewModel;
        SetBinding(TitleProperty, new Binding(nameof(ClientDetailsPageViewModel.PageTitle)));

        var titleLabel = new Label
        {
            FontAttributes = FontAttributes.Bold,
            FontSize = 28
        };
        titleLabel.SetBinding(Label.TextProperty, nameof(ClientDetailsPageViewModel.PageTitle));

        var summaryLabel = new Label
        {
            LineBreakMode = LineBreakMode.WordWrap
        };
        summaryLabel.SetBinding(Label.TextProperty, nameof(ClientDetailsPageViewModel.Summary));

        var activityIndicator = new ActivityIndicator
        {
            HorizontalOptions = LayoutOptions.Start
        };
        activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(ClientDetailsPageViewModel.IsBusy));
        activityIndicator.SetBinding(ActivityIndicator.IsVisibleProperty, nameof(ClientDetailsPageViewModel.IsBusy));

        var errorLabel = new Label
        {
            TextColor = Color.FromArgb("#B91C1C"),
            LineBreakMode = LineBreakMode.WordWrap
        };
        errorLabel.SetBinding(Label.TextProperty, nameof(ClientDetailsPageViewModel.ErrorMessage));

        var nameEntry = new Entry
        {
            Placeholder = "Client name"
        };
        nameEntry.SetBinding(Entry.TextProperty, nameof(ClientDetailsPageViewModel.Name));

        var contactNameEntry = new Entry
        {
            Placeholder = "Billing contact"
        };
        contactNameEntry.SetBinding(Entry.TextProperty, nameof(ClientDetailsPageViewModel.ContactName));

        var contactEmailEntry = new Entry
        {
            Placeholder = "billing@example.com",
            Keyboard = Keyboard.Email
        };
        contactEmailEntry.SetBinding(Entry.TextProperty, nameof(ClientDetailsPageViewModel.ContactEmail));

        var projectSummaryLabel = new Label
        {
            TextColor = Color.FromArgb("#475569"),
            LineBreakMode = LineBreakMode.WordWrap
        };
        projectSummaryLabel.SetBinding(Label.TextProperty, nameof(ClientDetailsPageViewModel.ActiveProjectSummary));

        var saveButton = new Button
        {
            Text = "Save Client"
        };
        saveButton.SetBinding(Button.CommandProperty, nameof(ClientDetailsPageViewModel.SaveCommand));

        var archiveButton = new Button
        {
            Text = "Archive Client",
            Background = new SolidColorBrush(Color.FromArgb("#FEE2E2")),
            TextColor = Color.FromArgb("#991B1B")
        };
        archiveButton.SetBinding(Button.CommandProperty, nameof(ClientDetailsPageViewModel.ArchiveCommand));
        archiveButton.SetBinding(IsVisibleProperty, nameof(ClientDetailsPageViewModel.CanArchive));

        var cancelButton = new Button
        {
            Text = "Back to Clients"
        };
        cancelButton.SetBinding(Button.CommandProperty, nameof(ClientDetailsPageViewModel.CancelCommand));

        Content = new Grid
        {
            Padding = 24,
            Children =
            {
                new ScrollView
                {
                    Content = new VerticalStackLayout
                    {
                        Spacing = 16,
                        Children =
                        {
                            titleLabel,
                            summaryLabel,
                            activityIndicator,
                            errorLabel,
                            CreateField("Client Name", nameEntry),
                            CreateField("Contact Name", contactNameEntry),
                            CreateField("Contact Email", contactEmailEntry),
                            new Border
                            {
                                Background = new SolidColorBrush(Color.FromArgb("#F8FAFC")),
                                Stroke = new SolidColorBrush(Color.FromArgb("#E2E8F0")),
                                StrokeShape = new RoundRectangle
                                {
                                    CornerRadius = new CornerRadius(16)
                                },
                                Padding = 16,
                                Content = projectSummaryLabel
                            },
                            saveButton,
                            archiveButton,
                            cancelButton
                        }
                    }
                }
            }
        };
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        viewModel.ApplyQueryAttributes(query);
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

    private static View CreateField(string labelText, InputView input)
    {
        return new VerticalStackLayout
        {
            Spacing = 8,
            Children =
            {
                new Label
                {
                    Text = labelText,
                    FontAttributes = FontAttributes.Bold
                },
                input
            }
        };
    }
}
