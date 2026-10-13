using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DesignFlow.Revit.Studio;

namespace DesignFlow.Revit.Ui;

public sealed class SettingsWindow : Window
{
    private readonly StudioSettings _settings;
    private readonly TextBox _api = new();
    private readonly TextBox _email = new();
    private readonly PasswordBox _password = new();
    private readonly ComboBox _projects = new();

    public SettingsWindow(StudioSettings settings)
    {
        _settings = settings;
        Title = "DesignFlow studio settings";
        Width = 480;
        Height = 360;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = new SolidColorBrush(Color.FromRgb(246, 244, 241));
        _api.Text = settings.ApiUrl;
        _email.Text = settings.Email;
        _password.Password = settings.Password;
        Content = Build();
        Loaded += async (_, _) => await LoadProjectsAsync();
    }

    private UIElement Build()
    {
        var root = new StackPanel { Margin = new Thickness(20) };
        root.Children.Add(Label("API"));
        root.Children.Add(_api);
        root.Children.Add(Label("Email"));
        root.Children.Add(_email);
        root.Children.Add(Label("Password"));
        root.Children.Add(_password);
        root.Children.Add(Label("Project"));
        root.Children.Add(_projects);
        var save = new Button
        {
            Content = "Save",
            Margin = new Thickness(0, 16, 0, 0),
            Padding = new Thickness(12, 8, 12, 8),
            Background = new SolidColorBrush(Color.FromRgb(79, 70, 229)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0)
        };
        save.Click += (_, _) => SaveAndClose();
        root.Children.Add(save);
        return root;
    }

    private async Task LoadProjectsAsync()
    {
        try
        {
            _settings.ApiUrl = _api.Text.Trim();
            _settings.Email = _email.Text.Trim();
            _settings.Password = _password.Password;
            var projects = await new StudioClient(_settings).ProjectsAsync();
            _projects.Items.Clear();
            foreach (var project in projects)
            {
                _projects.Items.Add(new ComboBoxItem
                {
                    Content = project.Code + " · " + project.Name,
                    Tag = project.ProjectId.ToString()
                });
            }

            for (var index = 0; index < _projects.Items.Count; index++)
            {
                if (_projects.Items[index] is ComboBoxItem item && item.Tag as string == _settings.ProjectId)
                {
                    _projects.SelectedIndex = index;
                    break;
                }
            }

            if (_projects.SelectedIndex < 0 && _projects.Items.Count > 0)
            {
                _projects.SelectedIndex = 0;
            }
        }
        catch (Exception exception)
        {
            _projects.Items.Add("Could not load projects: " + exception.Message);
        }
    }

    private void SaveAndClose()
    {
        _settings.ApiUrl = _api.Text.Trim();
        _settings.Email = _email.Text.Trim();
        _settings.Password = _password.Password;
        if (_projects.SelectedItem is ComboBoxItem item && item.Tag is string projectId)
        {
            _settings.ProjectId = projectId;
        }

        _settings.Save();
        DialogResult = true;
        Close();
    }

    private static TextBlock Label(string text) => new()
    {
        Text = text,
        Margin = new Thickness(0, 10, 0, 4),
        FontWeight = FontWeights.SemiBold
    };
}
