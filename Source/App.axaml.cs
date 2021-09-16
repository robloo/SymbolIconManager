using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.IO;

namespace IconManager
{
    public class App : Application
    {
        /// <summary>
        /// Contains a reference to the main window of the application.
        /// </summary>
        public static MainWindow MainWindow;

        /// <summary>
        /// Contains the directory to the root of the icon manager cache.
        /// This is commonly used for saving glyph images and source files.
        /// </summary>
        public static readonly string IconManagerCache = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            @"IconManagerCache");

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
