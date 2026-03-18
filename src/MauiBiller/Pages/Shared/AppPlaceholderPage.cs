using Microsoft.Maui.Controls.Shapes;

namespace MauiBiller.Pages.Shared;

public abstract class AppPlaceholderPage : ContentPage
{
    protected readonly record struct PlaceholderAction(string Label, string Route);

    protected AppPlaceholderPage(string pageTitle, string summary, params PlaceholderAction[] actions)
    {
        Title = pageTitle;

        var actionsLayout = new VerticalStackLayout
        {
            Spacing = 12
        };

        if (actions.Length is 0)
        {
            actionsLayout.Children.Add(new Label
            {
                Text = "This screen is scaffolded and ready for feature implementation.",
                LineBreakMode = LineBreakMode.WordWrap
            });
        }
        else
        {
            actionsLayout.Children.Add(new Label
            {
                Text = "Related placeholder routes",
                FontAttributes = FontAttributes.Bold
            });

            foreach (var action in actions)
            {
                var button = new Button
                {
                    Text = action.Label
                };

                button.Clicked += async (_, _) => await Shell.Current.GoToAsync(action.Route);
                actionsLayout.Children.Add(button);
            }
        }

        var card = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb("#F8FAFC")),
            Padding = 20,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(16)
            },
            Content = new VerticalStackLayout
            {
                Spacing = 16,
                Children =
                {
                    new Label
                    {
                        Text = pageTitle,
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold
                    },
                    new Label
                    {
                        Text = summary,
                        LineBreakMode = LineBreakMode.WordWrap
                    },
                    actionsLayout
                }
            }
        };

        Content = new Grid
        {
            Padding = 24,
            Children =
            {
                new ScrollView
                {
                    Content = card
                }
            }
        };
    }

    protected static PlaceholderAction CreateAction(string label, string route)
    {
        return new PlaceholderAction(label, route);
    }
}
