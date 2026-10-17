using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace IconManager
{
    /// <summary>
    /// The base class for all view models.
    /// </summary>
    public class ViewModelBase :
        ObservableObject,
        ISupportInitialize
    {
        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// Gets or sets a value indicating whether internal property change notifications are ignored
        /// and no processing such as dependent property calculation or synchronization is done.
        /// Regardless of this value, the <see cref="INotifyPropertyChanged.PropertyChanged"/> event
        /// will be invoked.
        /// </summary>
        /// <remarks>
        /// Property changes within the current view model are ignored by the view model itself but
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> events are still raised.
        /// </remarks>
        protected bool IsPropertyChangedIgnoredInternally { get; set; } = false;

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        /// <inheritdoc/>
        public virtual void BeginInit()
        {
            this.IsPropertyChangedIgnoredInternally = true;

            return;
        }

        /// <inheritdoc/>
        public virtual void EndInit()
        {
            this.IsPropertyChangedIgnoredInternally = false;

            return;
        }
    }
}
