using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System;

namespace IconManager.Services
{
    /// <summary>
    /// Implements <see cref="IStorageService"/> using Avalonia's storage provider.
    /// </summary>
    public class AvaloniaStorageService : IStorageService
    {
        /// <inheritdoc/>
        public IStorageProvider Provider
        {
            get
            {
                var lifetime = Application.Current?.ApplicationLifetime;

                if (lifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    return desktop.MainWindow!.StorageProvider;
                }

                if (lifetime is ISingleViewApplicationLifetime singleView)
                {
                    return TopLevel.GetTopLevel(singleView.MainView)!.StorageProvider;
                }

                throw new InvalidOperationException("No active platform lifecycle found.");
            }
        }
    }
}
