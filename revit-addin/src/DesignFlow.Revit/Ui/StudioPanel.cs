using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DesignFlow.Revit.Studio;

namespace DesignFlow.Revit.Ui;

public sealed class StudioPanel : Window
{
    public StudioPanel()
    {
        Title = "DesignFlow AI";
        Width = 320;
        Height = 420;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = new SolidColorBrush(Color.FromRgb(20, 18, 38));
        Content = Build();
    }

    private static UIElement Build()
    {
        var root = new StackPanel { Margin = new Thickness(18) };
        root.Children.Add(new TextBlock
        {
            Text = "DesignFlow plugins",
            Foreground = Brushes.White,
            FontSize = 20,
            FontWeight = FontWeights.Bold
        });
        root.Children.Add(new TextBlock
        {
            Text = "These tools read the open model and post results to the studio.",
            Foreground = new SolidColorBrush(Color.FromRgb(196, 181, 253)),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 16)
        });
        root.Children.Add(Action("Sync model", "sync"));
        root.Children.Add(Action("Run validation", "validate"));
        root.Children.Add(Action("Fetch requirements", "requirements"));
        root.Children.Add(Action("Queue translation", "translate"));
        root.Children.Add(Action("Settings", "settings"));
        return root;
    }

    private static Button Action(string label, string action)
    {
        var button = new Button
        {
            Content = label,
            Margin = new Thickness(0, 0, 0, 8),
            Padding = new Thickness(12, 10, 12, 10),
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Background = new SolidColorBrush(Color.FromRgb(79, 70, 229)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0)
        };
        button.Click += (_, _) => StudioRuntime.Raise(action);
        return button;
    }
}
