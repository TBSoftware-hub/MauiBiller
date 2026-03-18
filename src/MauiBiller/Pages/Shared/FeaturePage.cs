using Microsoft.Maui.Controls.Shapes;
using MauiBiller.ViewModels;

namespace MauiBiller.Pages.Shared;

public abstract class FeaturePage<TViewModel> : ContentPage
    where TViewModel : FeaturePageViewModel
{
    protected FeaturePage(TViewModel viewModel)
    {
        BindingContext = viewModel;
        SetBinding(TitleProperty, new Binding(nameof(FeaturePageViewModel.PageTitle)));

        var titleLabel = new Label
        {
            FontAttributes = FontAttributes.Bold,
            FontSize = 28
        };
        titleLabel.SetBinding(Label.TextProperty, nameof(FeaturePageViewModel.PageTitle));

        var summaryLabel = new Label
        {
            LineBreakMode = LineBreakMode.WordWrap
        };
        summaryLabel.SetBinding(Label.TextProperty, nameof(FeaturePageViewModel.Summary));

        var activityIndicator = new ActivityIndicator
        {
            HorizontalOptions = LayoutOptions.Start
        };
        activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(FeaturePageViewModel.IsBusy));
        activityIndicator.SetBinding(ActivityIndicator.IsVisibleProperty, nameof(FeaturePageViewModel.IsBusy));

        var metricsLayout = new VerticalStackLayout
        {
            Spacing = 12
        };

        BindableLayout.SetItemsSource(metricsLayout, viewModel.Metrics);
        BindableLayout.SetItemTemplate(metricsLayout, new DataTemplate(() =>
        {
            var label = new Label
            {
                FontAttributes = FontAttributes.Bold
            };
            label.SetBinding(Label.TextProperty, nameof(FeatureMetricViewModel.Label));

            var value = new Label
            {
                FontSize = 20,
                FontAttributes = FontAttributes.Bold
            };
            value.SetBinding(Label.TextProperty, nameof(FeatureMetricViewModel.Value));

            var detail = new Label
            {
                LineBreakMode = LineBreakMode.WordWrap
            };
            detail.SetBinding(Label.TextProperty, nameof(FeatureMetricViewModel.Detail));

            return new Border
            {
                Background = new SolidColorBrush(Color.FromArgb("#F8FAFC")),
                Padding = 16,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(16)
                },
                Content = new VerticalStackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        label,
                        value,
                        detail
                    }
                }
            };
        }));

        var actionsLayout = new VerticalStackLayout
        {
            Spacing = 12
        };

        BindableLayout.SetItemsSource(actionsLayout, viewModel.Actions);
        BindableLayout.SetItemTemplate(actionsLayout, new DataTemplate(() =>
        {
            var button = new Button();
            button.SetBinding(Button.TextProperty, nameof(FeatureActionViewModel.Title));
            button.SetBinding(Button.CommandProperty, nameof(FeatureActionViewModel.Command));

            var description = new Label
            {
                LineBreakMode = LineBreakMode.WordWrap
            };
            description.SetBinding(Label.TextProperty, nameof(FeatureActionViewModel.Description));

            return new Border
            {
                Background = new SolidColorBrush(Color.FromArgb("#FFFFFF")),
                Padding = 16,
                StrokeShape = new RoundRectangle
                {
                    CornerRadius = new CornerRadius(16)
                },
                Content = new VerticalStackLayout
                {
                    Spacing = 8,
                    Children =
                    {
                        button,
                        description
                    }
                }
            };
        }));

        Content = new Grid
        {
            Padding = 24,
            Children =
            {
                new ScrollView
                {
                    Content = new VerticalStackLayout
                    {
                        Spacing = 20,
                        Children =
                        {
                            titleLabel,
                            summaryLabel,
                            activityIndicator,
                            new Label
                            {
                                Text = "Architecture snapshot",
                                FontAttributes = FontAttributes.Bold,
                                FontSize = 18
                            },
                            metricsLayout,
                            new Label
                            {
                                Text = "Related routes",
                                FontAttributes = FontAttributes.Bold,
                                FontSize = 18
                            },
                            actionsLayout
                        }
                    }
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TViewModel viewModel)
        {
            await viewModel.EnsureInitializedAsync();
        }
    }
}
