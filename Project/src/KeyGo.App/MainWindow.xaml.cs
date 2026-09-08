using System.Windows;
using System.Windows.Controls;
using KeyGo.App.ViewModels;

namespace KeyGo.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(App.Host);
    }

    private void ProviderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is ProviderCardViewModel provider && DataContext is MainViewModel shell)
        {
            shell.Providers.Select(provider);
        }
    }

    private void ApiKeyBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel shell && sender is PasswordBox box)
        {
            shell.Providers.ApiKey = box.Password;
        }
    }
}