using Avalonia.Platform.Storage;

namespace IconManager.Services
{
    /// <summary>
    /// Provides access to the current storage provider.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// Gets the storage provider instance.
        /// </summary>
        IStorageProvider Provider { get; }
    }
}
