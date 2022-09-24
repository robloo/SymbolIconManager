using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace IconManager
{
    public partial class MainWindow : CoreWindow
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.DataContext = this;
        }
    }
}
