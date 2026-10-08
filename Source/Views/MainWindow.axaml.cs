using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;

namespace IconManager
{
    public partial class MainWindow : Window
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }
    }
}
