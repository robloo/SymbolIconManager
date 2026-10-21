using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using IconManager.Core.Glyphs;
using IconManager.IconSets;
using IconManager.Services;
using IconManager.ViewModels;
using IconManager.Views;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
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
        /// Gets the provider of all registered application services.
        /// </summary>
        /// <remarks>
        /// This is only available after the first Window or TopLevel is created.
        /// </remarks>
        public ServiceProvider Services { get; private set; } = null!;

        /// <summary>
        /// Gets the current instance of the <see cref="Application"/> class.
        /// </summary>
        /// <value>
        /// The current instance of the <see cref="Application"/> class.
        /// </value>
        public static new App Current
        {
            get => (App)Application.Current!;
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
            this.ConfigureServices();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow()
                {
                    DataContext = new ShellViewModel()
                };

                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

        /// <summary>
        /// Configures and registers services for dependency injection in the application.
        /// </summary>
        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Create the default glyph source provider
            var glyphSourceProvider = new GlyphSourceProviderCollection();
            glyphSourceProvider.Register(FluentUISystemGlyphs.Instance);
            glyphSourceProvider.Register(LineAwesomeGlyphs.Instance);
            glyphSourceProvider.Register(SegoeFluentGlyphs.Instance);
            glyphSourceProvider.Register(SegoeMDL2AssetsGlyphs.Instance);
            glyphSourceProvider.Register(SegoeUISymbolGlyphs.Instance);
            glyphSourceProvider.Register(WinJSSymbolsGlyphs.Instance);

            var glyphRenderer = new SkiaGlyphRenderer(glyphSourceProvider)
            {
                // Use a white background with black fill for best compatibility
                // This is visible across both light/dark themes easily and is best for exported images
                BackgroundColor = SKColors.White,
                FillColor       = SKColors.Black
            };

            services.AddSingleton<IGlyphSourceProvider>(glyphSourceProvider);
            services.AddSingleton<IGlyphRenderer<GlyphBitmap>>(glyphRenderer);
            services.AddSingleton<IStorageService>(new AvaloniaStorageService());

            this.Services = services.BuildServiceProvider();

            return;
        }
    }
}
