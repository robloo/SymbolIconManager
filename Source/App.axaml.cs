using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace IconManager
{
    public class App : Application
    {
        /// <summary>
        /// Contains a reference to the main window of the application.
        /// </summary>
        public static MainWindow MainWindow;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
                App.MainWindow = desktop.MainWindow as MainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
