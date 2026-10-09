using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.IO;

namespace IconManager
{
    public class App : Application
    {
        /// <summary>
        /// Contains the directory to the root of the icon manager cache.
        /// This is commonly used for saving glyph images and source files.
        /// </summary>
        public static readonly string IconManagerCache = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            @"IconManagerCache");

        /// <summary>
        /// Contains a reference to the main window of the application.
        /// </summary>
        public MainWindow? MainWindow { get; private set; }

        /// <summary>
        /// Contains a reference to the top level of the application.
        /// </summary>
        public TopLevel? TopLevel { get; private set; }

        /// <summary>
        /// Gets the current instance of the <see cref="Application"/> class.
        /// </summary>
        /// <value>
        /// The current instance of the <see cref="Application"/> class.
        /// </value>
        public static new App? Current
        {
            get => Application.Current as App;
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

#if DEBUG
            this.AttachDeveloperTools();
#endif
        }

        /// <inheritdoc/>
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow();

                desktop.MainWindow = mainWindow;
                App.Current?.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
