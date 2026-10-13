using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DesignFlow.Revit.Studio;

namespace DesignFlow.Revit.Ui;

public sealed class RequirementsWindow : Window
{
    public RequirementsWindow(ProjectDetailResponse project)
    {
        Title = "DesignFlow requirements";
        Width = 520;
        Height = 460;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = new SolidColorBrush(Color.FromRgb(246, 244, 241));
        Content = Build(project);
    }

    private static UIElement Build(ProjectDetailResponse project)
    {
        var root = new StackPanel { Margin = new Thickness(20) };
        root.Children.Add(TitleBlock(project.Code + " · " + project.Name));
        if (project.Requirements.Count == 0)
        {
            root.Children.Add(new TextBlock { Text = "This project has no requirements yet.", Margin = new Thickness(0, 12, 0, 0) });
            return root;
        }

        foreach (var requirement in project.Requirements)
        {
            var card = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 10, 0, 0)
            };
            var body = new StackPanel();
            body.Children.Add(new TextBlock { Text = requirement.Title, FontWeight = FontWeights.SemiBold });
            body.Children.Add(new TextBlock
            {
                Text = requirement.Category + " · " + requirement.Priority + " · " + requirement.Status,
                Foreground = Brushes.DimGray,
                Margin = new Thickness(0, 4, 0, 0)
            });
            card.Child = body;
            root.Children.Add(card);
        }

        return new ScrollViewer { Content = root };
    }

    private static TextBlock TitleBlock(string text) => new()
    {
        Text = text,
        FontSize = 20,
        FontWeight = FontWeights.Bold
    };
}
