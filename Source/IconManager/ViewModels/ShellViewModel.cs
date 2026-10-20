using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;

namespace IconManager.ViewModels
{
    /// <summary>
    /// The application-wide shell view model.
    /// </summary>
    public partial class ShellViewModel : ViewModelBase
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        public ShellViewModel()
        {
        }

        /***************************************************************************************
         *
         * Commands
         *
         ***************************************************************************************/

        /// <summary>
        /// Toggles the application-wide requested theme variant between light and dark.
        /// </summary>
        [RelayCommand]
        public void ToggleThemeVariant()
        {
            App.Current.RequestedThemeVariant =
                App.Current.ActualThemeVariant == ThemeVariant.Dark
                    ? ThemeVariant.Light
                    : ThemeVariant.Dark;

            return;
        }
    }
}
