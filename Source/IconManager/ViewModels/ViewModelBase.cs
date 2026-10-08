using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IconManager
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

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
    }
}
