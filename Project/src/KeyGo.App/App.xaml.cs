using System.Windows;
using System.Windows.Threading;
using KeyGo.App.Services;

namespace KeyGo.App;

public partial class App : System.Windows.Application
{
    public static AppHost Host { get; } = new();

    public App()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KeyGo-startup-error.txt");
        System.IO.File.WriteAllText(path, e.Exception.ToString());
        System.Windows.MessageBox.Show($"KeyGo could not start. Details were written to:\n{path}", "KeyGo", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}