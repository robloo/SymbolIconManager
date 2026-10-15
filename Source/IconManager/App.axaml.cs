using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using IconManager.Core.Glyphs;
using IconManager.IconSets;
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
        /// Gets a reference to the main window of the application.
        /// </summary>
        public MainWindow? MainWindow { get; private set; }

        /// <summary>
        /// Gets a reference to the top level of the application.
        /// </summary>
        public TopLevel? TopLevel { get; private set; }

        /// <summary>
        /// Gets the default glyph source provider for all supported icon sets.
        /// </summary>
        public IGlyphSourceProvider GlyphSourceProvider { get; private set; } = null!;

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
            // Initialize the default glyph source provider
            var provider = new GlyphSourceProviderCollection();
            provider.Register(FluentUISystemGlyphs.Instance);
            provider.Register(LineAwesomeGlyphs.Instance);
            provider.Register(SegoeFluentGlyphs.Instance);
            provider.Register(SegoeMDL2AssetsGlyphs.Instance);
            provider.Register(SegoeUISymbolGlyphs.Instance);
            provider.Register(WinJSSymbolsGlyphs.Instance);
            this.GlyphSourceProvider = provider;

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
