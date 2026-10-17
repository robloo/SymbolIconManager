using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IconManager
{
    /// <summary>
    /// The base class for all view models.
    /// </summary>
    public class ViewModelBase :
        INotifyPropertyChanged,
        ISupportInitialize
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

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

        /// <summary>
        /// Sets the value of a field and raises a property changed event.
        /// </summary>
        /// <remarks>
        /// [CallerMemberName] is not used to automatically set propertyName so this code can more
        /// easily be converted as needed.
        /// </remarks>
        /// <typeparam name="T">They type of the field value to set.</typeparam>
        /// <param name="field">The field to set.</param>
        /// <param name="value">The new value of the field.</param>
        /// <param name="propertyName">The name of the property being set.</param>
        /// <returns>True if the field was set, otherwise false.</returns>
        protected bool SetField<T>(
            ref T field,
            T value,
            [CallerMemberName] string propertyName = "")
        {
            if (object.Equals(field, value))
            {
                return false;
            }
            else
            {
                field = value;
                this.OnPropertyChanged(propertyName);
            }

            return true;
        }

        /// <summary>
        /// Raises the property changed event for the given property name.
        /// </summary>
        /// <param name="propertyName">The name of the property being changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return;
        }

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
